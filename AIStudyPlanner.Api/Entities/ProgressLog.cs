namespace AIStudyPlanner.Api.Entities;

public class ProgressLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid StudyTaskId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursSpent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public StudyTask? StudyTask { get; set; }
    public StudyGoal? StudyGoal { get; set; }
}
