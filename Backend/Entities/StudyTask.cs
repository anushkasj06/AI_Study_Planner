namespace AIStudyPlanner.Api.Entities;

public class StudyTask
{
    public Guid Id { get; set; }
    public Guid StudyPlanId { get; set; }
    public Guid StudyGoalId { get; set; }
    public Guid UserId { get; set; }
    public DateTime TaskDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public TaskType TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public GoalPriority Priority { get; set; } = GoalPriority.Medium;
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public StudyPlan? StudyPlan { get; set; }
    public StudyGoal? StudyGoal { get; set; }
    public User? User { get; set; }
    public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
