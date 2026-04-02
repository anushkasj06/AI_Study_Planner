using System.Net.Http.Json;
using System.Text.Json;
using AIStudyPlanner.Api.DTOs.Plans;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Helpers;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace AIStudyPlanner.Api.Services;

public class GeminiService(
    HttpClient httpClient,
    IOptions<GeminiOptions> geminiOptions,
    ILogger<GeminiService> logger) : IGeminiService
{
    private readonly GeminiOptions _options = geminiOptions.Value;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GeneratedPlanResult> GenerateStudyPlanAsync(User user, StudyGoal goal, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(goal);

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("Gemini API key missing, using fallback sample JSON.");
            return BuildFallback(prompt, goal);
        }

        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    topK = 10,
                    topP = 0.8,
                    responseMimeType = "application/json"
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";
            using var response = await httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("Gemini call failed with status {StatusCode}: {Body}", response.StatusCode, rawResponse);
                throw new InvalidOperationException("AI schedule generation failed. Please verify Gemini settings and try again.");
            }

            var jsonPayload = ExtractJson(rawResponse);
            var result = ParsePlan(jsonPayload);
            result.RawPrompt = prompt;
            result.RawResponse = rawResponse;
            return result;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected Gemini integration failure.");
            throw new InvalidOperationException("AI schedule generation failed. Please try again shortly.");
        }
    }

    private static string BuildPrompt(StudyGoal goal) =>
        $"""
Create a structured study plan for a student. Return ONLY valid JSON. No markdown, no explanation.
The JSON should contain:
- planTitle
- startDate
- endDate
- totalEstimatedHours
- tasks: array of
  - taskDate
  - topic
  - subtopic
  - estimatedHours
  - taskType
  - notes
  - priority

The plan should:
- balance topics across available days
- respect dailyAvailableHours
- include revision and practice sessions
- include lighter schedule before difficult days if possible
- be realistic and student-friendly

Student goal:
- title: {goal.Title}
- description: {goal.Description}
- targetDate: {goal.TargetDate:yyyy-MM-dd}
- dailyAvailableHours: {goal.DailyAvailableHours}
- difficultyLevel: {goal.DifficultyLevel}
- priority: {goal.Priority}
- preferredStudyTime: {goal.PreferredStudyTime}
- breakPreference: {goal.BreakPreference}
- subjects/topics: {goal.SubjectsJson}
""";

    private static string ExtractJson(string rawResponse)
    {
        using var document = JsonDocument.Parse(rawResponse);
        var text = document.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("AI returned an empty response.");
        }

        return text;
    }

    private static GeneratedPlanResult ParsePlan(string jsonPayload)
    {
        var result = JsonSerializer.Deserialize<GeneratedPlanResult>(jsonPayload, JsonOptions)
            ?? throw new InvalidOperationException("AI returned invalid JSON.");

        if (string.IsNullOrWhiteSpace(result.PlanTitle) || result.Tasks.Count == 0)
        {
            throw new InvalidOperationException("AI returned incomplete plan data.");
        }

        if (result.Tasks.Any(x => x.TaskDate == default || string.IsNullOrWhiteSpace(x.Topic) || x.EstimatedHours <= 0))
        {
            throw new InvalidOperationException("AI returned task data that does not match the required schema.");
        }

        return result;
    }

    private static GeneratedPlanResult BuildFallback(string prompt, StudyGoal goal)
    {
        var start = DateTime.UtcNow.Date;
        var tasks = new List<GeneratedTaskItem>
        {
            new()
            {
                TaskDate = start,
                Topic = "Foundation",
                Subtopic = $"{goal.Title} kickoff",
                EstimatedHours = Math.Min(goal.DailyAvailableHours, 2m),
                TaskType = "Learn",
                Notes = "Review scope, resources, and warm up.",
                Priority = goal.Priority.ToString()
            },
            new()
            {
                TaskDate = start.AddDays(1),
                Topic = "Core Concepts",
                Subtopic = "Guided concept building",
                EstimatedHours = Math.Min(goal.DailyAvailableHours, 2.5m),
                TaskType = "Practice",
                Notes = "Pair reading with exercises.",
                Priority = goal.Priority.ToString()
            },
            new()
            {
                TaskDate = start.AddDays(2),
                Topic = "Revision",
                Subtopic = "Recall and self-test",
                EstimatedHours = Math.Min(goal.DailyAvailableHours, 1.5m),
                TaskType = "Revise",
                Notes = "Keep the session lighter and reflective.",
                Priority = "Medium"
            }
        };

        return new GeneratedPlanResult
        {
            PlanTitle = $"{goal.Title} Smart Plan",
            StartDate = start,
            EndDate = goal.TargetDate.Date,
            TotalEstimatedHours = tasks.Sum(x => x.EstimatedHours),
            Tasks = tasks,
            RawPrompt = prompt,
            RawResponse = JsonSerializer.Serialize(new
            {
                planTitle = $"{goal.Title} Smart Plan",
                startDate = start,
                endDate = goal.TargetDate.Date,
                totalEstimatedHours = tasks.Sum(x => x.EstimatedHours),
                tasks
            }),
            UsedFallback = true
        };
    }
}
