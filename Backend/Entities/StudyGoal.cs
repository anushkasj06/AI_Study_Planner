namespace AIStudyPlanner.Api.Entities;

public class StudyGoal
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public decimal DailyAvailableHours { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public GoalPriority Priority { get; set; }
    public PreferredStudyTime PreferredStudyTime { get; set; }
    public BreakPreference BreakPreference { get; set; }
    public string SubjectsJson { get; set; } = "[]";
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public ICollection<StudyPlan> StudyPlans { get; set; } = new List<StudyPlan>();
    public ICollection<StudyTask> StudyTasks { get; set; } = new List<StudyTask>();
    public ICollection<ProgressLog> ProgressLogs { get; set; } = new List<ProgressLog>();
    public ICollection<AiRequestLog> AiRequestLogs { get; set; } = new List<AiRequestLog>();
}
