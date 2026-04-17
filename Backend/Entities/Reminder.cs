namespace AIStudyPlanner.Api.Entities;

public class Reminder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;
    public ReminderDeliveryStatus DeliveryStatus { get; set; } = ReminderDeliveryStatus.Pending;
    public int DeliveryAttempts { get; set; }
    public DateTime? LastDeliveryAttemptAtUtc { get; set; }
    public string LastDeliveryError { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public StudyTask? StudyTask { get; set; }
}
