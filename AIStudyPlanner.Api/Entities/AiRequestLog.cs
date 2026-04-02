namespace AIStudyPlanner.Api.Entities;

public class AiRequestLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? StudyGoalId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public AiRequestStatus Status { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public StudyGoal? StudyGoal { get; set; }
}
