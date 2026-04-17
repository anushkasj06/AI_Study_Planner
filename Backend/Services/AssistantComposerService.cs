using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Assistant;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Helpers;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AIStudyPlanner.Api.Services;

public sealed class AssistantComposerService(
    ApplicationDbContext dbContext,
    IHttpClientFactory httpClientFactory,
    IOptions<GroqOptions> groqOptions,
    IOptions<GeminiOptions> geminiOptions,
    ILogger<AssistantComposerService> logger) : IAssistantComposerService
{
    private readonly GroqOptions _groqOptions = groqOptions.Value;
    private readonly GeminiOptions _geminiOptions = geminiOptions.Value;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AssistantChatResponse> CreateChatResponseAsync(Guid userId, string message, CancellationToken cancellationToken = default)
    {
        var context = await BuildContextAsync(userId, null, cancellationToken);
        var prompt = BuildChatPrompt(context, message);
        var payload = await TryGeneratePayloadAsync(prompt, context, AssistantMode.Chat, cancellationToken);

        if (payload is null)
        {
            return BuildFallbackChatResponse(context, message);
        }

        if (string.IsNullOrWhiteSpace(payload.Provider))
        {
            payload.Provider = "Groq";
        }

        if (string.IsNullOrWhiteSpace(payload.MindMapMermaid) && ShouldGenerateMindMap(message))
        {
            payload.MindMapMermaid = BuildFallbackMindMap(context, message);
        }

        return new AssistantChatResponse
        {
            Reply = payload.Reply,
            Suggestions = payload.Suggestions,
            MindMapMermaid = payload.MindMapMermaid,
            NoteTitle = payload.NoteTitle,
            NoteMarkdown = payload.NoteMarkdown,
            Provider = payload.Provider,
            UsedFallback = payload.UsedFallback
        };
    }

    public async Task<AssistantNoteDraftResponse> CreateNoteDraftAsync(Guid userId, Guid? studyGoalId, string prompt, CancellationToken cancellationToken = default)
    {
        var context = await BuildContextAsync(userId, studyGoalId, cancellationToken);
        var draftPrompt = BuildNotePrompt(context, prompt);
        var payload = await TryGeneratePayloadAsync(draftPrompt, context, AssistantMode.Note, cancellationToken);

        if (payload is null)
        {
            return BuildFallbackNoteDraft(context, prompt);
        }

        if (string.IsNullOrWhiteSpace(payload.Provider))
        {
            payload.Provider = "Groq";
        }

        if (string.IsNullOrWhiteSpace(payload.MindMapMermaid))
        {
            payload.MindMapMermaid = BuildFallbackMindMap(context, prompt);
        }

        return new AssistantNoteDraftResponse
        {
            Title = string.IsNullOrWhiteSpace(payload.NoteTitle) ? BuildFallbackNoteDraft(context, prompt).Title : payload.NoteTitle,
            ContentMarkdown = string.IsNullOrWhiteSpace(payload.NoteMarkdown) ? BuildFallbackNoteDraft(context, prompt).ContentMarkdown : payload.NoteMarkdown,
            MindMapMermaid = string.IsNullOrWhiteSpace(payload.MindMapMermaid) ? BuildFallbackNoteDraft(context, prompt).MindMapMermaid : payload.MindMapMermaid,
            Provider = payload.Provider,
            UsedFallback = payload.UsedFallback
        };
    }

    private async Task<AssistantPayload?> TryGeneratePayloadAsync(
        string prompt,
        AssistantContextSnapshot context,
        AssistantMode mode,
        CancellationToken cancellationToken)
    {
        var groqAttempt = await TryGenerateWithGroqAsync(prompt, mode, cancellationToken);
        if (groqAttempt is not null)
        {
            return groqAttempt;
        }

        var geminiAttempt = await TryGenerateWithGeminiAsync(prompt, mode, cancellationToken);
        if (geminiAttempt is not null)
        {
            return geminiAttempt;
        }

        logger.LogWarning("Assistant providers unavailable. Using local fallback assistant response.");
        return null;
    }

    private async Task<AssistantPayload?> TryGenerateWithGroqAsync(string prompt, AssistantMode mode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_groqOptions.ApiKey))
        {
            logger.LogWarning("Groq API key is not configured. Trying Gemini assistant fallback.");
            return null;
        }

        if (!Uri.TryCreate(_groqOptions.Endpoint, UriKind.Absolute, out var endpointUri))
        {
            logger.LogWarning("Groq assistant endpoint is invalid. Trying Gemini assistant fallback.");
            return null;
        }

        var timeoutSeconds = NormalizeTimeoutSeconds(_groqOptions.RequestTimeoutSeconds);
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            var requestBody = new
            {
                model = ResolveGroqModel(),
                temperature = 0.35,
                max_tokens = mode == AssistantMode.Note ? 2400 : 1800,
                response_format = new
                {
                    type = "json_object"
                },
                messages = new object[]
                {
                    new
                    {
                        role = "system",
                        content = BuildSystemPrompt(mode)
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                }
            };

            var client = httpClientFactory.CreateClient(nameof(AssistantComposerService));
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpointUri)
            {
                Content = JsonContent.Create(requestBody)
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqOptions.ApiKey);

            using var response = await client.SendAsync(requestMessage, timeoutCts.Token);
            var rawResponse = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Groq assistant request failed with status {StatusCode}: {Body}", (int)response.StatusCode, Truncate(rawResponse, 500));
                return null;
            }

            var content = ExtractGroqText(rawResponse);
            var normalizedJson = NormalizeJsonPayload(content);
            var payload = ParsePayload(normalizedJson, mode);
            payload.Provider = "Groq";
            payload.UsedFallback = false;
            return payload;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Groq assistant request timed out after {TimeoutSeconds} seconds.", timeoutSeconds);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Groq assistant request failed. Trying Gemini assistant fallback.");
            return null;
        }
    }

    private async Task<AssistantPayload?> TryGenerateWithGeminiAsync(string prompt, AssistantMode mode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_geminiOptions.ApiKey))
        {
            logger.LogWarning("Gemini API key is not configured. Using local fallback assistant response.");
            return null;
        }

        var timeoutSeconds = 25;
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
                            new { text = BuildGeminiPrompt(prompt, mode) }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.35,
                    topK = 20,
                    topP = 0.9,
                    responseMimeType = "application/json"
                }
            };

            var modelName = NormalizeGeminiModel(_geminiOptions.Model);
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelName}:generateContent?key={_geminiOptions.ApiKey}";
            var client = httpClientFactory.CreateClient(nameof(AssistantComposerService));
            using var response = await client.PostAsJsonAsync(url, requestBody, timeoutCts.Token);
            var rawResponse = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Gemini assistant request failed with status {StatusCode}: {Body}", (int)response.StatusCode, Truncate(rawResponse, 500));
                return null;
            }

            var content = ExtractGeminiText(rawResponse);
            var normalizedJson = NormalizeJsonPayload(content);
            var payload = ParsePayload(normalizedJson, mode);
            payload.Provider = "Gemini";
            payload.UsedFallback = false;
            return payload;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Gemini assistant request timed out after {TimeoutSeconds} seconds.", timeoutSeconds);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gemini assistant request failed: {Message}", ex.Message);
            return null;
        }
    }

    private async Task<AssistantContextSnapshot> BuildContextAsync(Guid userId, Guid? studyGoalId, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");

        IQueryable<StudyGoal> goalQuery = dbContext.StudyGoals
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (studyGoalId.HasValue)
        {
            goalQuery = goalQuery.Where(x => x.Id == studyGoalId.Value);
        }

        goalQuery = goalQuery.OrderByDescending(x => x.UpdatedAt);

        var goals = await goalQuery
            .Take(5)
            .ToListAsync(cancellationToken);

        var activeGoalIds = goals.Select(x => x.Id).ToList();
        var recentProgress = await dbContext.ProgressLogs
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= DateTime.UtcNow.Date.AddDays(-7))
            .OrderByDescending(x => x.Date)
            .Take(10)
            .ToListAsync(cancellationToken);

        var upcomingTasks = await dbContext.StudyTasks
            .AsNoTracking()
            .Where(x => x.UserId == userId && !x.IsCompleted)
            .OrderBy(x => x.TaskDate)
            .Take(8)
            .ToListAsync(cancellationToken);

        var upcomingReminders = await dbContext.Reminders
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ReminderDateTime >= DateTime.UtcNow)
            .OrderBy(x => x.ReminderDateTime)
            .Take(5)
            .ToListAsync(cancellationToken);

        var weeklyHours = recentProgress.Sum(x => x.HoursSpent);
        var completedTasks = await dbContext.StudyTasks
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId && x.IsCompleted, cancellationToken);
        var totalTasks = await dbContext.StudyTasks
            .AsNoTracking()
            .CountAsync(x => x.UserId == userId, cancellationToken);

        var topGoal = goals.FirstOrDefault();
        return new AssistantContextSnapshot
        {
            UserId = userId,
            UserName = user.FullName,
            UserEmail = user.Email,
            Goals = goals.Select(goal => new AssistantGoalSnapshot
            {
                Id = goal.Id,
                Title = goal.Title,
                TargetDate = goal.TargetDate,
                Priority = goal.Priority.ToString(),
                DifficultyLevel = goal.DifficultyLevel.ToString(),
                Subjects = DeserializeSubjects(goal.SubjectsJson)
            }).ToList(),
            TopGoalId = topGoal?.Id,
            TopGoalTitle = topGoal?.Title,
            WeeklyHours = weeklyHours,
            CompletedTasks = completedTasks,
            TotalTasks = totalTasks,
            PendingTasks = Math.Max(totalTasks - completedTasks, 0),
            UpcomingTasks = upcomingTasks.Select(task => new AssistantTaskSnapshot
            {
                Id = task.Id,
                Title = task.Topic,
                Subtopic = task.Subtopic,
                TaskDate = task.TaskDate,
                Priority = task.Priority.ToString(),
                TaskType = task.TaskType.ToString()
            }).ToList(),
            UpcomingReminders = upcomingReminders.Select(reminder => new AssistantReminderSnapshot
            {
                Id = reminder.Id,
                Title = reminder.Title,
                ReminderDateTime = reminder.ReminderDateTime,
                Channel = reminder.Channel.ToString(),
                DeliveryStatus = reminder.DeliveryStatus.ToString()
            }).ToList()
        };
    }

    private static AssistantChatResponse BuildFallbackChatResponse(AssistantContextSnapshot context, string userMessage)
    {
        var goalTitle = context.TopGoalTitle ?? "your current study goal";
        var asksForMindMap = ShouldGenerateMindMap(userMessage);
        var reply = userMessage.Contains("mind map", StringComparison.OrdinalIgnoreCase)
            ? $"I can map {goalTitle} into a clean study graph. Use the prompt below and I’ll keep the structure balanced around core concepts, practice, recall, and review."
            : $"I’m tuned to your workspace, {context.UserName}. Your top focus is {goalTitle}, with {context.PendingTasks} pending tasks and {context.WeeklyHours:0.#} hours logged this week. The best next move is a focused block on the highest-impact topic, then a short recall pass.\n\nIf you want, ask me for a mind map, a revision note, or a study reminder and I’ll format it around your current goals.";

        var suggestionGoalPayload = context.TopGoalId?.ToString();
        var suggestions = new List<AssistantSuggestion>
        {
            new() { Type = "action", Label = "Create a mind map", Payload = suggestionGoalPayload },
            new() { Type = "action", Label = "Write a revision note", Payload = suggestionGoalPayload },
            new() { Type = "action", Label = "Plan my next study block", Payload = suggestionGoalPayload },
            new() { Type = "action", Label = "Set a reminder", Payload = suggestionGoalPayload }
        };

        return new AssistantChatResponse
        {
            Reply = reply,
            Suggestions = suggestions,
            MindMapMermaid = asksForMindMap ? BuildFallbackMindMap(context, userMessage) : string.Empty,
            NoteTitle = string.Empty,
            NoteMarkdown = string.Empty,
            Provider = "Fallback",
            UsedFallback = true
        };
    }

    private static AssistantNoteDraftResponse BuildFallbackNoteDraft(AssistantContextSnapshot context, string prompt)
    {
        var title = string.IsNullOrWhiteSpace(context.TopGoalTitle)
            ? "AI Study Note"
            : $"{context.TopGoalTitle} - Study Note";

        var promptText = prompt.Trim();
        var content = new StringBuilder();
        content.AppendLine($"# {title}");
        content.AppendLine();
        content.AppendLine($"Prompt: {promptText}");
        content.AppendLine();
        content.AppendLine("## Summary");
        content.AppendLine("- Define the goal in one sentence.");
        content.AppendLine("- Keep the explanation short, exam-focused, and active-recall friendly.");
        content.AppendLine();
        content.AppendLine("## Key Points");
        content.AppendLine("- Core concept: the central idea you must remember.");
        content.AppendLine("- Practice: one worked example or problem type.");
        content.AppendLine("- Recall: one fast test question.");
        content.AppendLine();
        content.AppendLine("## Revision Loop");
        content.AppendLine("1. Read once for structure.");
        content.AppendLine("2. Close the page and recall the outline.");
        content.AppendLine("3. Revisit after 24 to 48 hours.");

        return new AssistantNoteDraftResponse
        {
            Title = title,
            ContentMarkdown = content.ToString(),
            MindMapMermaid = BuildFallbackMindMap(context, promptText),
            Provider = "Fallback",
            UsedFallback = true
        };
    }

    private static AssistantPayload ParsePayload(string json, AssistantMode mode)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (mode == AssistantMode.Chat)
        {
            return new AssistantPayload
            {
                Reply = GetString(root, "reply") ?? "I could not generate a response.",
                Suggestions = GetSuggestions(root),
                MindMapMermaid = GetString(root, "mindMapMermaid") ?? string.Empty,
                NoteTitle = GetString(root, "noteTitle") ?? string.Empty,
                NoteMarkdown = GetString(root, "noteMarkdown") ?? string.Empty
            };
        }

        return new AssistantPayload
        {
            NoteTitle = GetString(root, "title") ?? string.Empty,
            NoteMarkdown = GetString(root, "contentMarkdown") ?? string.Empty,
            MindMapMermaid = GetString(root, "mindMapMermaid") ?? string.Empty
        };
    }

    private static List<AssistantSuggestion> GetSuggestions(JsonElement root)
    {
        if (!root.TryGetProperty("suggestions", out var suggestionsElement) || suggestionsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var suggestions = new List<AssistantSuggestion>();
        foreach (var item in suggestionsElement.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var label = item.GetString();
                if (!string.IsNullOrWhiteSpace(label))
                {
                    suggestions.Add(new AssistantSuggestion
                    {
                        Type = "action",
                        Label = label,
                        Payload = null
                    });
                }

                continue;
            }

            if (item.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            suggestions.Add(new AssistantSuggestion
            {
                Type = GetString(item, "type") ?? "action",
                Label = GetString(item, "label") ?? string.Empty,
                Payload = GetString(item, "payload")
            });
        }

        return suggestions;
    }

    private static string BuildSystemPrompt(AssistantMode mode)
    {
        var sharedRules = "Use the user's study context. Be specific, supportive, and action-oriented. Return valid JSON only. No markdown fences. No prose outside JSON.";

        return mode switch
        {
            AssistantMode.Chat => $"You are a personalized study assistant inside AI Study Planner. {sharedRules} The JSON must contain reply, suggestions, mindMapMermaid, noteTitle, noteMarkdown. Reply should be warm and concise. Suggestions should include 3 or 4 actionable items. mindMapMermaid should be empty unless the user asks for a map or a structured visual. If you do return a diagram, prefer Mermaid flowchart TB with a clean balanced layout and classDef styling. noteTitle and noteMarkdown can be empty unless useful.",
            AssistantMode.Note => $"You are generating a study note draft and mind map for AI Study Planner. {sharedRules} The JSON must contain title, contentMarkdown, mindMapMermaid. Write polished Markdown with short headings and bullets. The mind map should be a Mermaid flowchart TB or mindmap with a central node, 4 to 6 branches, and balanced graph design. Include classDef styling where helpful.",
            _ => sharedRules
        };
    }

    private static string BuildGeminiPrompt(string prompt, AssistantMode mode)
    {
        var modeInstruction = mode == AssistantMode.Chat
            ? "Return JSON with reply, suggestions, mindMapMermaid, noteTitle, noteMarkdown. mindMapMermaid must be empty unless the user explicitly asks for a mind map, graph, or visual structure."
            : "Return JSON with title, contentMarkdown, mindMapMermaid.";

        return $"You are a personalized study assistant for AI Study Planner. Use the user's study context and reply in valid JSON only. No markdown fences. No extra text. {modeInstruction} The output must be useful, specific, and not generic.\n\nPrompt:\n{prompt}";
    }

    private static string BuildChatPrompt(AssistantContextSnapshot context, string message)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Student: {context.UserName}");
        sb.AppendLine($"Email: {context.UserEmail}");
        sb.AppendLine($"Current message: {message.Trim()}");
        sb.AppendLine();
        sb.AppendLine("Study context:");
        sb.AppendLine($"- Weekly study hours: {context.WeeklyHours:0.#}");
        sb.AppendLine($"- Completed tasks: {context.CompletedTasks}");
        sb.AppendLine($"- Pending tasks: {context.PendingTasks}");
        sb.AppendLine($"- Active goals: {FormatGoals(context.Goals)}");
        sb.AppendLine($"- Upcoming tasks: {FormatTasks(context.UpcomingTasks)}");
        sb.AppendLine($"- Upcoming reminders: {FormatReminders(context.UpcomingReminders)}");
        sb.AppendLine();
        sb.AppendLine("Instruction:");
        sb.AppendLine("Respond as a personal study coach. Reference the user's top goal when relevant. If the message asks for a mind map, structure, graph, note, or plan, return a Mermaid diagram that looks clean on a dark or light canvas.");
        sb.AppendLine("Use the user's first name naturally if appropriate. Keep the reply concise and useful.");
        return sb.ToString();
    }

    private static string BuildNotePrompt(AssistantContextSnapshot context, string prompt)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Student: {context.UserName}");
        sb.AppendLine($"Top goal: {context.TopGoalTitle ?? "None"}");
        sb.AppendLine($"Prompt: {prompt.Trim()}");
        sb.AppendLine();
        sb.AppendLine("Context:");
        sb.AppendLine($"- Weekly hours: {context.WeeklyHours:0.#}");
        sb.AppendLine($"- Active goals: {FormatGoals(context.Goals)}");
        sb.AppendLine($"- Upcoming tasks: {FormatTasks(context.UpcomingTasks)}");
        sb.AppendLine();
        sb.AppendLine("Instruction:");
        sb.AppendLine("Write a study note with a concise title, a short summary, key points, recall prompts, and a revision loop. Generate a Mermaid mind map with a central root node and visually balanced branches.");
        return sb.ToString();
    }

    private static string BuildFallbackMindMap(AssistantContextSnapshot context, string? prompt = null)
    {
        var root = SanitizeMermaidText(context.TopGoalTitle ?? "Study Focus");
        var promptText = SanitizeMermaidText(string.IsNullOrWhiteSpace(prompt) ? "Personalized study plan" : prompt);
        var subjects = context.Goals.FirstOrDefault()?.Subjects;
        var branches = subjects is { Count: > 0 }
            ? subjects.Take(5).ToList()
            : ["Concepts", "Practice", "Recall", "Revision"];

        var sb = new StringBuilder();
        sb.AppendLine("flowchart TB");
        sb.AppendLine("    classDef root fill:#0f766e,color:#ffffff,stroke:#0f766e,stroke-width:2px;");
        sb.AppendLine("    classDef branch fill:#eef6ff,color:#0f172a,stroke:#60a5fa,stroke-width:1.5px;");
        sb.AppendLine($"    Root(({root}))");
        sb.AppendLine($"    Prompt[\"{promptText}\"]");
        sb.AppendLine("    Root --> Prompt");

        for (var index = 0; index < branches.Count; index++)
        {
            var branch = SanitizeMermaidText(branches[index]);
            var branchId = $"Branch{index + 1}";
            sb.AppendLine($"    Root --> {branchId}[\"{branch}\"]");
            sb.AppendLine($"    {branchId} --> {branchId}a[\"Learn\"]");
            sb.AppendLine($"    {branchId} --> {branchId}b[\"Practice\"]");
            sb.AppendLine($"    {branchId} --> {branchId}c[\"Recall\"]");
            sb.AppendLine($"    class {branchId} branch;");
        }

        sb.AppendLine("    class Root root;");
        sb.AppendLine("    class Prompt branch;");
        return sb.ToString();
    }

    private static bool ShouldGenerateMindMap(string message)
    {
        var lower = message.ToLowerInvariant();
        return lower.Contains("mind map") ||
               lower.Contains("mindmap") ||
               lower.Contains("graph") ||
               lower.Contains("visual") ||
               lower.Contains("structure") ||
               lower.Contains("map my") ||
               lower.Contains("organize");
    }

    private string ResolveGroqModel()
    {
        var configuredModel = _groqOptions.Model.Trim();
        if (string.IsNullOrWhiteSpace(configuredModel) || configuredModel.StartsWith("groq/", StringComparison.OrdinalIgnoreCase))
        {
            return "llama-3.3-70b-versatile";
        }

        return configuredModel;
    }

    private static string NormalizeGeminiModel(string model)
    {
        var configuredModel = model.Trim();
        if (string.IsNullOrWhiteSpace(configuredModel))
        {
            return "gemini-2.0-flash";
        }

        return configuredModel;
    }

    private static string FormatGoals(IReadOnlyCollection<AssistantGoalSnapshot> goals)
    {
        if (goals.Count == 0)
        {
            return "none yet";
        }

        return string.Join(" | ", goals.Select(goal => $"{goal.Title} ({goal.Priority}, {goal.DifficultyLevel})"));
    }

    private static string FormatTasks(IReadOnlyCollection<AssistantTaskSnapshot> tasks)
    {
        if (tasks.Count == 0)
        {
            return "none";
        }

        return string.Join(" | ", tasks.Select(task => $"{task.Title} on {task.TaskDate:dd MMM}"));
    }

    private static string FormatReminders(IReadOnlyCollection<AssistantReminderSnapshot> reminders)
    {
        if (reminders.Count == 0)
        {
            return "none";
        }

        return string.Join(" | ", reminders.Select(reminder => $"{reminder.Title} at {reminder.ReminderDateTime:dd MMM HH:mm}"));
    }

    private static List<string> DeserializeSubjects(string subjectsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(subjectsJson, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string ExtractGroqText(string rawResponse)
    {
        using var document = JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Groq assistant did not return choices.");
        }

        var choice = choices[0];
        if (!choice.TryGetProperty("message", out var messageElement) ||
            !messageElement.TryGetProperty("content", out var contentElement))
        {
            throw new InvalidOperationException("Groq assistant did not return message content.");
        }

        var content = contentElement.GetString();
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Groq assistant returned empty content.");
        }

        return content;
    }

    private static string ExtractGeminiText(string rawResponse)
    {
        using var document = JsonDocument.Parse(rawResponse);

        if (!document.RootElement.TryGetProperty("candidates", out var candidates) ||
            candidates.ValueKind != JsonValueKind.Array ||
            candidates.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini assistant did not return candidates.");
        }

        var candidate = candidates[0];
        if (!candidate.TryGetProperty("content", out var contentElement) ||
            !contentElement.TryGetProperty("parts", out var parts) ||
            parts.ValueKind != JsonValueKind.Array ||
            parts.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("Gemini assistant did not return content parts.");
        }

        var text = parts[0].GetProperty("text").GetString();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Gemini assistant returned empty text content.");
        }

        return text;
    }

    private static string NormalizeJsonPayload(string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            throw new InvalidOperationException("Assistant provider returned empty content.");
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

        throw new InvalidOperationException("Assistant provider returned malformed JSON.");
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
        }

        return normalized.Trim();
    }

    private static string ExtractFirstJsonBlock(string payload)
    {
        var start = payload.IndexOf('{');
        if (start < 0)
        {
            return string.Empty;
        }

        var depth = 0;
        for (var index = start; index < payload.Length; index++)
        {
            switch (payload[index])
            {
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return payload[start..(index + 1)];
                    }
                    break;
            }
        }

        return string.Empty;
    }

    private static string TryRepairUnclosedJson(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        var trimmed = payload.Trim();
        var openBraces = trimmed.Count(ch => ch == '{');
        var closeBraces = trimmed.Count(ch => ch == '}');
        var missingBraces = openBraces - closeBraces;
        if (missingBraces <= 0)
        {
            return string.Empty;
        }

        return trimmed + new string('}', missingBraces);
    }

    private static bool IsValidJson(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        try
        {
            using var _ = JsonDocument.Parse(payload);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
        {
            return property.GetString();
        }

        return null;
    }

    private static string SanitizeMermaidText(string text)
    {
        return text
            .Replace("\"", string.Empty)
            .Replace("(", string.Empty)
            .Replace(")", string.Empty)
            .Replace("[", string.Empty)
            .Replace("]", string.Empty)
            .Replace("{", string.Empty)
            .Replace("}", string.Empty)
            .Trim();
    }

    private static int NormalizeTimeoutSeconds(int timeoutSeconds)
    {
        if (timeoutSeconds < 5)
        {
            return 5;
        }

        return Math.Min(timeoutSeconds, 120);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private enum AssistantMode
    {
        Chat,
        Note
    }

    private sealed class AssistantContextSnapshot
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public IReadOnlyCollection<AssistantGoalSnapshot> Goals { get; set; } = [];
        public Guid? TopGoalId { get; set; }
        public string? TopGoalTitle { get; set; }
        public decimal WeeklyHours { get; set; }
        public int CompletedTasks { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public IReadOnlyCollection<AssistantTaskSnapshot> UpcomingTasks { get; set; } = [];
        public IReadOnlyCollection<AssistantReminderSnapshot> UpcomingReminders { get; set; } = [];
    }

    private sealed class AssistantGoalSnapshot
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime TargetDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public IReadOnlyCollection<string> Subjects { get; set; } = [];
    }

    private sealed class AssistantTaskSnapshot
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtopic { get; set; } = string.Empty;
        public DateTime TaskDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string TaskType { get; set; } = string.Empty;
    }

    private sealed class AssistantReminderSnapshot
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReminderDateTime { get; set; }
        public string Channel { get; set; } = string.Empty;
        public string DeliveryStatus { get; set; } = string.Empty;
    }

    private sealed class AssistantPayload
    {
        public string Reply { get; set; } = string.Empty;
        public List<AssistantSuggestion> Suggestions { get; set; } = [];
        public string MindMapMermaid { get; set; } = string.Empty;
        public string NoteTitle { get; set; } = string.Empty;
        public string NoteMarkdown { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public bool UsedFallback { get; set; }
    }
}
