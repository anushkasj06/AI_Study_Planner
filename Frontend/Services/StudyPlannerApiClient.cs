using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIStudyPlanner.Web.Models;
using Microsoft.Extensions.Options;

namespace AIStudyPlanner.Web.Services;

public sealed class StudyPlannerApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public StudyPlannerApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor, IOptions<ApiOptions> options)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _httpClient.BaseAddress = new Uri(options.Value.BaseUrl, UriKind.Absolute);
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public Task<AuthResponse> LoginAsync(LoginViewModel model)
    {
        return SendAndReadAsync<AuthResponse>(HttpMethod.Post, "/api/auth/login", new
        {
            model.Email,
            model.Password
        });
    }

    public Task<AuthResponse> RegisterAsync(RegisterViewModel model)
    {
        return SendAndReadAsync<AuthResponse>(HttpMethod.Post, "/api/auth/register", new
        {
            model.FullName,
            model.Email,
            model.Password
        });
    }

    public Task<UserProfileResponse> MeAsync() => GetAsync<UserProfileResponse>("/api/auth/me");

    public Task<DashboardSummaryResponse> DashboardSummaryAsync() => GetAsync<DashboardSummaryResponse>("/api/dashboard/summary");

    public Task<ProgressSummaryResponse> ProgressSummaryAsync() => GetAsync<ProgressSummaryResponse>("/api/progress/summary");

    public Task<List<StudyGoalResponse>> GoalsAsync() => GetAsync<List<StudyGoalResponse>>("/api/goals");

    public Task<StudyGoalResponse> GoalByIdAsync(Guid goalId) => GetAsync<StudyGoalResponse>($"/api/goals/{goalId}");

    public Task<StudyGoalResponse> CreateGoalAsync(StudyGoalCreateRequest request)
        => SendAndReadAsync<StudyGoalResponse>(HttpMethod.Post, "/api/goals", request);

    public Task<StudyPlanResponse> GeneratePlanAsync(Guid goalId, bool regenerate)
        => SendAndReadAsync<StudyPlanResponse>(HttpMethod.Post, $"/api/ai/{(regenerate ? "regenerate-plan" : "generate-plan")}/{goalId}");

    public Task<List<StudyPlanResponse>> PlansByGoalAsync(Guid goalId) => GetAsync<List<StudyPlanResponse>>($"/api/plans/goal/{goalId}");

    public Task<List<StudyTaskResponse>> TodayTasksAsync() => GetAsync<List<StudyTaskResponse>>("/api/tasks/today");

    public Task<List<StudyTaskResponse>> WeekTasksAsync() => GetAsync<List<StudyTaskResponse>>("/api/tasks/week");

    public Task<StudyTaskResponse> ToggleTaskAsync(Guid taskId) => SendAndReadAsync<StudyTaskResponse>(new HttpMethod("PATCH"), $"/api/tasks/{taskId}/toggle-complete");

    public Task DeleteTaskAsync(Guid taskId) => SendAsync(HttpMethod.Delete, $"/api/tasks/{taskId}");

    public Task<List<ReminderResponse>> RemindersAsync() => GetAsync<List<ReminderResponse>>("/api/reminders");

    public Task<ReminderResponse> CreateReminderAsync(ReminderCreateRequest request)
        => SendAndReadAsync<ReminderResponse>(HttpMethod.Post, "/api/reminders", request);

    public Task<ReminderResponse> MarkReminderReadAsync(Guid reminderId)
        => SendAndReadAsync<ReminderResponse>(new HttpMethod("PATCH"), $"/api/reminders/{reminderId}/mark-read");

    public Task DeleteReminderAsync(Guid reminderId) => SendAsync(HttpMethod.Delete, $"/api/reminders/{reminderId}");

    private async Task<T> GetAsync<T>(string path)
    {
        using var response = await SendAsyncInternal(HttpMethod.Get, path);
        return await ReadAsync<T>(response);
    }

    private async Task<T> SendAndReadAsync<T>(HttpMethod method, string path, object? body = null)
    {
        using var response = await SendAsyncInternal(method, path, body);
        return await ReadAsync<T>(response);
    }

    private async Task SendAsync(HttpMethod method, string path, object? body = null)
    {
        using var response = await SendAsyncInternal(method, path, body);
        await EnsureSuccessAsync(response);
    }

    private async Task<HttpResponseMessage> SendAsyncInternal(HttpMethod method, string path, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        AttachAuthorizationHeader(request);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return await _httpClient.SendAsync(request);
    }

    private void AttachAuthorizationHeader(HttpRequestMessage request)
    {
        var token = _httpContextAccessor.HttpContext?.User.FindFirstValue("access_token");
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        await EnsureSuccessAsync(response);

        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            return default!;
        }

        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();
        var message = ExtractMessage(content);
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
            ? $"Request failed with status {(int)response.StatusCode}."
            : message);
    }

    private static string ExtractMessage(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
            {
                return detail.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                return title.GetString() ?? string.Empty;
            }

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                var messages = new List<string>();
                foreach (var property in errors.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        messages.AddRange(property.Value.EnumerateArray()
                            .Select(item => item.GetString())
                            .Where(item => !string.IsNullOrWhiteSpace(item))!);
                    }
                }

                return string.Join(" ", messages);
            }
        }
        catch
        {
            // Ignore parsing failures and use the raw payload instead.
        }

        return content;
    }
}