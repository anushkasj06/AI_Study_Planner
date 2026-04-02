using AIStudyPlanner.Api.Entities;
using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Tasks;

public class StudyTaskUpdateRequest
{
    public DateTime TaskDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public TaskType TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public GoalPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
}

public class StudyTaskResponse
{
    public Guid Id { get; set; }
    public Guid StudyPlanId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime TaskDate { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public TaskType TaskType { get; set; }
    public string Notes { get; set; } = string.Empty;
    public GoalPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
}

public class StudyTaskUpdateRequestValidator : FluentValidation.AbstractValidator<StudyTaskUpdateRequest>
{
    public StudyTaskUpdateRequestValidator()
    {
        RuleFor(x => x.Topic).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Subtopic).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EstimatedHours).InclusiveBetween(0.25m, 16m);
        RuleFor(x => x.ActualHours).InclusiveBetween(0m, 24m);
        RuleFor(x => x.Notes).MaximumLength(1200);
    }
}
