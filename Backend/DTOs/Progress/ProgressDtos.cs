using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Progress;

public class ProgressLogCreateRequest
{
    public Guid StudyTaskId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursSpent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
}

public class ProgressLogResponse
{
    public Guid Id { get; set; }
    public Guid StudyTaskId { get; set; }
    public Guid StudyGoalId { get; set; }
    public DateTime Date { get; set; }
    public decimal HoursSpent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProgressSummaryResponse
{
    public decimal CompletedHoursThisWeek { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public int DailyStreak { get; set; }
    public decimal WeeklyProgressPercentage { get; set; }
}

public class GoalProgressResponse
{
    public Guid GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public decimal GoalCompletionPercentage { get; set; }
    public decimal HoursSpent { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
}

public class StreakResponse
{
    public int CurrentStreak { get; set; }
}

public class ProgressLogCreateRequestValidator : FluentValidation.AbstractValidator<ProgressLogCreateRequest>
{
    public ProgressLogCreateRequestValidator()
    {
        RuleFor(x => x.StudyTaskId).NotEmpty();
        RuleFor(x => x.StudyGoalId).NotEmpty();
        RuleFor(x => x.HoursSpent).InclusiveBetween(0.25m, 24m);
        RuleFor(x => x.CompletionPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
