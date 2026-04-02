using AIStudyPlanner.Api.Entities;
using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Goals;

public class StudyGoalCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public decimal DailyAvailableHours { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public GoalPriority Priority { get; set; }
    public PreferredStudyTime PreferredStudyTime { get; set; }
    public BreakPreference BreakPreference { get; set; }
    public List<string> Subjects { get; set; } = [];
    public GoalStatus Status { get; set; } = GoalStatus.Active;
    public bool AutoGeneratePlan { get; set; }
}

public class StudyGoalUpdateRequest : StudyGoalCreateRequest;

public class StudyGoalResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public decimal DailyAvailableHours { get; set; }
    public DifficultyLevel DifficultyLevel { get; set; }
    public GoalPriority Priority { get; set; }
    public PreferredStudyTime PreferredStudyTime { get; set; }
    public BreakPreference BreakPreference { get; set; }
    public IReadOnlyCollection<string> Subjects { get; set; } = [];
    public GoalStatus Status { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class StudyGoalCreateRequestValidator : FluentValidation.AbstractValidator<StudyGoalCreateRequest>
{
    public StudyGoalCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1500);
        RuleFor(x => x.TargetDate).GreaterThan(DateTime.UtcNow.Date.AddDays(-1));
        RuleFor(x => x.DailyAvailableHours).InclusiveBetween(0.5m, 16m);
        RuleFor(x => x.Subjects).NotEmpty();
        RuleForEach(x => x.Subjects).NotEmpty().MaximumLength(100);
    }
}

public class StudyGoalUpdateRequestValidator : FluentValidation.AbstractValidator<StudyGoalUpdateRequest>
{
    public StudyGoalUpdateRequestValidator()
    {
        Include(new StudyGoalCreateRequestValidator());
    }
}
