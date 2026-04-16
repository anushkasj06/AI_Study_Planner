using AIStudyPlanner.Api.DTOs.Tasks;

namespace AIStudyPlanner.Api.DTOs.Plans;

public class StudyPlanResponse
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

public class GeneratedPlanResult
{
    public string PlanTitle { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalEstimatedHours { get; set; }
    public List<GeneratedTaskItem> Tasks { get; set; } = [];
    public string RawPrompt { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
    public bool UsedFallback { get; set; }
}

public class GeneratedTaskItem
{
    public DateTime TaskDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public decimal EstimatedHours { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
}
