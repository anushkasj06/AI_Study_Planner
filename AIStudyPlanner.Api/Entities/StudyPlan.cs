namespace AIStudyPlanner.Api.Entities;

public class StudyPlan
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid StudyGoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalEstimatedHours { get; set; }
    public bool GeneratedByAI { get; set; }
    public string RawAiPrompt { get; set; } = string.Empty;
    public string RawAiResponse { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public StudyGoal? StudyGoal { get; set; }
    public ICollection<StudyTask> Tasks { get; set; } = new List<StudyTask>();
}
