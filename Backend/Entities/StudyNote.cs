namespace AIStudyPlanner.Api.Entities;

public class StudyNote
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? StudyGoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public string MindMapMermaid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public StudyGoal? StudyGoal { get; set; }
}
