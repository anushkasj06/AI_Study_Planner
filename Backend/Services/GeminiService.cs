using System.Net;
using System.Net.Http.Headers;
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
    IOptions<GroqOptions> groqOptions,
    ILogger<GeminiService> logger) : IGeminiService
{
    private readonly GeminiOptions _geminiOptions = geminiOptions.Value;
    private readonly GroqOptions _groqOptions = groqOptions.Value;

    private const int MinTimeoutSeconds = 5;
    private const int MaxTimeoutSeconds = 120;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<GeneratedPlanResult> GenerateStudyPlanAsync(User user, StudyGoal goal, string planningContext, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(goal, planningContext);

        var geminiAttempt = await TryGenerateWithGeminiAsync(prompt, cancellationToken);
        if (geminiAttempt.Success && geminiAttempt.Plan is not null)
        {
            geminiAttempt.Plan.RawPrompt = prompt;
            geminiAttempt.Plan.RawResponse = geminiAttempt.RawResponse;
            geminiAttempt.Plan.UsedFallback = false;
            return geminiAttempt.Plan;
        }

        logger.LogWarning("Gemini provider unavailable: {Reason}", geminiAttempt.ErrorMessage);

        var groqAttempt = await TryGenerateWithGroqAsync(prompt, cancellationToken);
        if (groqAttempt.Success && groqAttempt.Plan is not null)
        {
            groqAttempt.Plan.RawPrompt = prompt;
            groqAttempt.Plan.RawResponse = groqAttempt.RawResponse;
            groqAttempt.Plan.UsedFallback = false;
            return groqAttempt.Plan;
        }

        logger.LogWarning("Groq provider unavailable: {Reason}", groqAttempt.ErrorMessage);
        logger.LogWarning(
            "Using deterministic local plan fallback. GeminiError={GeminiError}; GroqError={GroqError}",
            geminiAttempt.ErrorMessage,
            groqAttempt.ErrorMessage);

        return BuildFallback(
            prompt,
            goal,
            geminiAttempt.ErrorMessage,
            groqAttempt.ErrorMessage,
            geminiAttempt.RawResponse,
            groqAttempt.RawResponse);
    }

    private async Task<ProviderAttemptResult> TryGenerateWithGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_geminiOptions.ApiKey))
        {
            return ProviderAttemptResult.Failed("Gemini API key is not configured.");
        }

        var timeoutSeconds = NormalizeTimeoutSeconds(_geminiOptions.RequestTimeoutSeconds);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

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

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_geminiOptions.Model}:generateContent?key={_geminiOptions.ApiKey}";
            using var response = await httpClient.PostAsJsonAsync(url, requestBody, timeoutCts.Token);
            var rawResponse = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var reason = BuildProviderErrorMessage("Gemini", response.StatusCode, rawResponse);
                logger.LogWarning("{Reason}", reason);
                return ProviderAttemptResult.Failed(reason, rawResponse);
            }

            var content = ExtractGeminiText(rawResponse);
            var normalizedJson = NormalizeJsonPayload(content);
            var plan = ParsePlan(normalizedJson);
            logger.LogInformation("Study plan generated using Gemini model {Model}.", _geminiOptions.Model);
            return ProviderAttemptResult.Succeeded(plan, rawResponse);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var reason = $"Gemini request timed out after {timeoutSeconds} seconds.";
            logger.LogWarning(ex, "{Reason}", reason);
            return ProviderAttemptResult.Failed(reason);
        }
        catch (Exception ex)
        {
            var reason = $"Gemini provider error: {ex.Message}";
            logger.LogWarning(ex, "Gemini request failed.");
            return ProviderAttemptResult.Failed(reason);
        }
    }

    private async Task<ProviderAttemptResult> TryGenerateWithGroqAsync(string prompt, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_groqOptions.ApiKey))
        {
            return ProviderAttemptResult.Failed("Groq API key is not configured.");
        }

        if (!Uri.TryCreate(_groqOptions.Endpoint, UriKind.Absolute, out var endpointUri))
        {
            return ProviderAttemptResult.Failed("Groq endpoint is invalid.");
        }

        var timeoutSeconds = NormalizeTimeoutSeconds(_groqOptions.RequestTimeoutSeconds);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var requestBody = new
            {
                model = _groqOptions.Model,
                temperature = 0.2,
                max_tokens = 3500,
                response_format = new
                {
                    type = "json_object"
                },
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = "You are a strict JSON planner. Return valid JSON only, no markdown, no prose."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
            };

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUri)
            {
                Content = JsonContent.Create(requestBody)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqOptions.ApiKey);

            using var response = await httpClient.SendAsync(requestMessage, timeoutCts.Token);
            var rawResponse = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var reason = BuildProviderErrorMessage("Groq", response.StatusCode, rawResponse);
                logger.LogWarning("{Reason}", reason);
                return ProviderAttemptResult.Failed(reason, rawResponse);
            }

            var content = ExtractGroqText(rawResponse);
            var normalizedJson = NormalizeJsonPayload(content);
            var plan = ParsePlan(normalizedJson);
            logger.LogInformation("Study plan generated using Groq model {Model}.", _groqOptions.Model);
            return ProviderAttemptResult.Succeeded(plan, rawResponse);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var reason = $"Groq request timed out after {timeoutSeconds} seconds.";
            logger.LogWarning(ex, "{Reason}", reason);
            return ProviderAttemptResult.Failed(reason);
        }
        catch (Exception ex)
        {
            var reason = $"Groq provider error: {ex.Message}";
            logger.LogWarning(ex, "Groq request failed.");
            return ProviderAttemptResult.Failed(reason);
        }
    }

    private static string BuildPrompt(StudyGoal goal, string planningContext) =>
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
- adapt plan intensity to recent learner momentum from historical context
- add weekly recap slots when progression is low
- contain 8 to 14 tasks total
- if the date range is long, group work into milestone-based sessions instead of daily entries

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

Historical planning context:
- {planningContext}
""";

    private static string ExtractGeminiText(string rawResponse)
    {
        using var document = JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini provider did not return candidates.");
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var contentElement) ||
            !contentElement.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini provider did not return content parts.");
        }

        var text = parts[0].GetProperty("text").GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini provider returned empty text content.");
        }

        return text;
    }

    private static string ExtractGroqText(string rawResponse)
    {
        using var document = JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Groq provider did not return choices.");
        }

        var choice = choices[0];
        if (!choice.TryGetProperty("message", out var message) ||
            !message.TryGetProperty("content", out var contentElement))
        {
            throw new InvalidOperationException("Groq provider did not return a message content payload.");
        }

        var content = contentElement.GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Groq provider returned empty content.");
        }

        return content;
    }

    private static string NormalizeJsonPayload(string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            throw new InvalidOperationException("AI provider returned empty payload.");
        }

        var normalized = StripMarkdownCodeFence(rawPayload.Trim());

        if (IsValidJson(normalized))
        {
            return normalized;
        }

        var extractedJson = ExtractFirstJsonBlock(normalized);
        if (!string.IsNullOrWhiteSpace(extractedJson))
        {
            if (IsValidJson(extractedJson))
            {
                return extractedJson;
            }

            var repairedJson = TryRepairUnclosedJson(extractedJson);
            if (!string.IsNullOrWhiteSpace(repairedJson))
            {
                return repairedJson;
            }
        }

        throw new InvalidOperationException("AI provider returned malformed JSON payload.");
    }

    private static string StripMarkdownCodeFence(string payload)
    {
        var normalized = payload;

        if (normalized.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineBreak = normalized.IndexOf('\n');
            if (firstLineBreak >= 0)
            {
                normalized = normalized[(firstLineBreak + 1)..];
            }

            if (normalized.EndsWith("```", StringComparison.Ordinal))
            {
                normalized = normalized[..^3];
            }

            normalized = normalized.Trim();
        }

        return normalized;
    }

    private static bool IsValidJson(string candidate)
    {
        try
        {
            using var _ = JsonDocument.Parse(candidate);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string ExtractFirstJsonBlock(string content)
    {
        var objectStart = content.IndexOf('{');
        var arrayStart = content.IndexOf('[');

        var start = -1;
        if (objectStart >= 0 && arrayStart >= 0)
        {
            start = Math.Min(objectStart, arrayStart);
        }
        else if (objectStart >= 0)
        {
            start = objectStart;
        }
        else if (arrayStart >= 0)
        {
            start = arrayStart;
        }

        if (start < 0)
        {
            return string.Empty;
        }

        var stack = new Stack<char>();
        var inString = false;
        var escaped = false;

        for (var i = start; i < content.Length; i += 1)
        {
            var ch = content[i];

            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
            {
                stack.Push('}');
                continue;
            }

            if (ch == '[')
            {
                stack.Push(']');
                continue;
            }

            if (ch == '}' || ch == ']')
            {
                if (stack.Count == 0 || stack.Peek() != ch)
                {
                    return content[start..(i + 1)];
                }

                stack.Pop();
                if (stack.Count == 0)
                {
                    return content[start..(i + 1)];
                }
            }
        }

        return content[start..];
    }

    private static string? TryRepairUnclosedJson(string partialJson)
    {
        var stack = new Stack<char>();
        var inString = false;
        var escaped = false;

        foreach (var ch in partialJson)
        {
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                    continue;
                }

                if (ch == '\\')
                {
                    escaped = true;
                    continue;
                }

                if (ch == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (ch == '"')
            {
                inString = true;
                continue;
            }

            if (ch == '{')
            {
                stack.Push('}');
                continue;
            }

            if (ch == '[')
            {
                stack.Push(']');
                continue;
            }

            if (ch == '}' || ch == ']')
            {
                if (stack.Count == 0 || stack.Peek() != ch)
                {
                    return null;
                }

                stack.Pop();
            }
        }

        if (inString)
        {
            return null;
        }

        if (stack.Count == 0)
        {
            return IsValidJson(partialJson) ? partialJson : null;
        }

        var suffix = new string(stack.ToArray());
        var repaired = partialJson + suffix;
        return IsValidJson(repaired) ? repaired : null;
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

    private static string BuildProviderErrorMessage(string providerName, HttpStatusCode statusCode, string body)
    {
        if (statusCode == HttpStatusCode.TooManyRequests)
        {
            return $"{providerName} quota or rate limit exceeded (HTTP 429).";
        }

        if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
        {
            return $"{providerName} API key is invalid or missing permissions (HTTP {(int)statusCode}).";
        }

        return $"{providerName} call failed with status {(int)statusCode} ({statusCode}). Body: {Truncate(body, 1000)}";
    }

    private static int NormalizeTimeoutSeconds(int timeoutSeconds)
    {
        return Math.Clamp(timeoutSeconds, MinTimeoutSeconds, MaxTimeoutSeconds);
    }

    private static GeneratedPlanResult BuildFallback(
        string prompt,
        StudyGoal goal,
        string geminiError,
        string groqError,
        string geminiRawResponse,
        string groqRawResponse)
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
                provider = "deterministic-fallback",
                geminiError,
                groqError,
                geminiRawResponse = Truncate(geminiRawResponse, 2000),
                groqRawResponse = Truncate(groqRawResponse, 2000),
                planTitle = $"{goal.Title} Smart Plan",
                startDate = start,
                endDate = goal.TargetDate.Date,
                totalEstimatedHours = tasks.Sum(x => x.EstimatedHours),
                tasks
            }),
            UsedFallback = true
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private sealed record ProviderAttemptResult(bool Success, GeneratedPlanResult? Plan, string ErrorMessage, string RawResponse)
    {
        public static ProviderAttemptResult Succeeded(GeneratedPlanResult plan, string rawResponse) =>
            new(true, plan, string.Empty, rawResponse);

        public static ProviderAttemptResult Failed(string errorMessage, string rawResponse = "") =>
            new(false, null, errorMessage, rawResponse);
    }
}
