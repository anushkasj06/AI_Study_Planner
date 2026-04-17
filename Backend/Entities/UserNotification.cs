namespace AIStudyPlanner.Api.Entities;

public class UserNotification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ReminderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.System;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Reminder? Reminder { get; set; }
}
