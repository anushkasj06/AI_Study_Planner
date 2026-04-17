namespace AIStudyPlanner.Web.Models;

public enum GoalStatus
{
    Draft,
    Active,
    Completed,
    Archived
}

public enum GoalPriority
{
    Low,
    Medium,
    High
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum PreferredStudyTime
{
    Morning,
    Afternoon,
    Evening,
    Flexible
}

public enum BreakPreference
{
    Pomodoro,
    ShortBreaks,
    LongBreaks,
    Minimal
}

public enum TaskType
{
    Learn,
    Revise,
    Practice,
    Test
}

public enum ReminderChannel
{
    InApp,
    Email,
    BrowserPush
}

public enum ReminderDeliveryStatus
{
    Pending,
    Sent,
    Failed
}

public enum NotificationType
{
    Reminder,
    StudyInsight,
    System
}

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserProfileResponse User { get; set; } = new();
}

public sealed class UserProfileResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudyGoalCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public decimal DailyAvailableHours { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public GoalPriority Priority { get; set; }
    public PreferredStudyTime PreferredStudyTime { get; set; }
    public BreakPreference BreakPreference { get; set; }
    public List<string> Subjects { get; set; } = [];
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public bool AutoGeneratePlan { get; set; }
}

public sealed class StudyGoalUpdateRequest : StudyGoalCreateRequest;

public sealed class StudyGoalResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public decimal DailyAvailableHours { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public GoalPriority Priority { get; set; }
    public PreferredStudyTime PreferredStudyTime { get; set; }
    public BreakPreference BreakPreference { get; set; }
    public IReadOnlyCollection<string> Subjects { get; set; } = [];
    public GoalStatus Status { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class StudyTaskUpdateRequest
{
    public DateTime TaskDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public TaskType TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public GoalPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
}

public sealed class StudyTaskResponse
{
    public Guid Id { get; set; }
    public Guid StudyPlanId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime TaskDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public TaskType TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public GoalPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
}

public sealed class StudyPlanResponse
{
    public Guid Id { get; set; }
    public Guid StudyGoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalEstimatedHours { get; set; }
    public bool GeneratedByAI { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyCollection<StudyTaskResponse> Tasks { get; set; } = [];
}

public sealed class AiProviderStatusResponse
{
    public string Provider { get; set; } = "Unknown";
    public string RequestStatus { get; set; } = "None";
    public bool UsedFallback { get; set; }
    public Guid? StudyGoalId { get; set; }
    public DateTime? LastAttemptAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ReminderCreateRequest
{
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;
}

public sealed class ReminderBatchCreateRequest
{
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public List<ReminderChannel> Channels { get; set; } = [];
}

public sealed class AiReminderDraftRequest
{
    public Guid? StudyTaskId { get; set; }
    public Guid? StudyGoalId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public DateTime? PreferredReminderDateTime { get; set; }
    public bool PreferEmail { get; set; }
    public bool PreferBrowserPush { get; set; }
}

public sealed class AiReminderDraftResponse
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public IReadOnlyCollection<ReminderChannel> RecommendedChannels { get; set; } = [];
    public string Reasoning { get; set; } = string.Empty;
}

public sealed class ReminderPipelineActionResponse
{
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class ReminderUpdateRequest : ReminderCreateRequest
{
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
}

public sealed class ReminderResponse
{
    public Guid Id { get; set; }
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
    public ReminderChannel Channel { get; set; }
    public ReminderDeliveryStatus DeliveryStatus { get; set; }
    public int DeliveryAttempts { get; set; }
    public DateTime? LastDeliveryAttemptAtUtc { get; set; }
    public string LastDeliveryError { get; set; } = string.Empty;
}

public sealed class ProgressSummaryResponse
{
    public decimal CompletedHoursThisWeek { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public int DailyStreak { get; set; }
    public decimal WeeklyProgressPercentage { get; set; }
}

public sealed class GoalProgressResponse
{
    public Guid GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public decimal GoalCompletionPercentage { get; set; }
    public decimal HoursSpent { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
}

public sealed class DashboardSummaryResponse
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public decimal HoursStudiedThisWeek { get; set; }
    public int StreakCount { get; set; }
    public int UnreadNotifications { get; set; }
    public IReadOnlyCollection<UpcomingReminderItem> UpcomingReminders { get; set; } = [];
    public IReadOnlyCollection<GoalProgressWidget> ProgressByGoal { get; set; } = [];
}

public sealed class UpcomingReminderItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public bool IsRead { get; set; }
}

public sealed class GoalProgressWidget
{
    public Guid GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public decimal CompletionPercentage { get; set; }
}

public sealed class ProgressLogCreateRequest
{
    public Guid StudyTaskId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursSpent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
}

public sealed class ProgressLogResponse
{
    public Guid Id { get; set; }
    public Guid StudyTaskId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursSpent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class NotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class WebPushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public sealed class WebPushPublicKeyResponse
{
    public string PublicKey { get; set; } = string.Empty;
}

public sealed class AssistantChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public sealed class AssistantChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<AssistantSuggestion> Suggestions { get; set; } = [];
    public string MindMapMermaid { get; set; } = string.Empty;
    public string NoteTitle { get; set; } = string.Empty;
    public string NoteMarkdown { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool UsedFallback { get; set; }
}

public sealed class AssistantSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Payload { get; set; }
}

public sealed class CreateNoteFromAssistantRequest
{
    public Guid? StudyGoalId { get; set; }
    public string Prompt { get; set; } = string.Empty;
}

public sealed class StudyNoteResponse
{
    public Guid Id { get; set; }
    public Guid? StudyGoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public string MindMapMermaid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}