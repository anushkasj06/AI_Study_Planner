namespace AIStudyPlanner.Api.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<StudyGoal> StudyGoals { get; set; } = new List<StudyGoal>();
    public ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
    public ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    public ICollection<AiRequestLog> AiRequestLogs { get; set; } = new List<AiRequestLog>();
    public ICollection<UserNotification> Notifications { get; set; } = new List<UserNotification>();
    public ICollection<WebPushSubscription> WebPushSubscriptions { get; set; } = new List<WebPushSubscription>();
    public ICollection<StudyNote> StudyNotes { get; set; } = new List<StudyNote>();
}
