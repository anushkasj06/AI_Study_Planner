using System.ComponentModel.DataAnnotations;

namespace AIStudyPlanner.Web.Models;

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = "demo@student.com";

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "Demo@12345";
}

public sealed class RegisterViewModel
{
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}

public sealed class GoalFormViewModel
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1500, MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string TargetDate { get; set; } = DateTime.Today.AddDays(30).ToString("yyyy-MM-dd");

    [Range(typeof(decimal), "0.5", "16")]
    public decimal DailyAvailableHours { get; set; } = 2;

    public DifficultyLevel DifficultyLevel { get; set; } = DifficultyLevel.Intermediate;

    public GoalPriority Priority { get; set; } = GoalPriority.High;

    public PreferredStudyTime PreferredStudyTime { get; set; } = PreferredStudyTime.Evening;

    public BreakPreference BreakPreference { get; set; } = BreakPreference.Pomodoro;

    [Required]
    public string Subjects { get; set; } = string.Empty;

    public GoalStatus Status { get; set; } = GoalStatus.Active;

    public bool AutoGeneratePlan { get; set; } = true;
}

public sealed class ReminderFormViewModel
{
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(500, MinimumLength = 5)]
    public string Message { get; set; } = string.Empty;

    [Required]
    public string ReminderDateTime { get; set; } = DateTime.Now.AddHours(1).ToString("yyyy-MM-ddTHH:mm");

    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;
}

public sealed class DashboardPageViewModel
{
    public DashboardSummaryResponse Dashboard { get; set; } = new();
    public ProgressSummaryResponse Progress { get; set; } = new();
}

public sealed class GoalsPageViewModel
{
    public IReadOnlyList<StudyGoalResponse> Goals { get; set; } = [];
}

public sealed class GoalDetailsPageViewModel
{
    public StudyGoalResponse Goal { get; set; } = new();
    public IReadOnlyList<StudyPlanResponse> Plans { get; set; } = [];
}

public sealed class PlannerPageViewModel
{
    public IReadOnlyList<StudyTaskResponse> TodayTasks { get; set; } = [];
    public IReadOnlyList<StudyTaskResponse> WeekTasks { get; set; } = [];
}

public sealed class ProgressPageViewModel
{
    public ProgressSummaryResponse Summary { get; set; } = new();
    public IReadOnlyList<StudyGoalResponse> Goals { get; set; } = [];
}

public sealed class RemindersPageViewModel
{
    public ReminderFormViewModel Form { get; set; } = new();
    public IReadOnlyList<ReminderResponse> Reminders { get; set; } = [];
}

public sealed class ProfilePageViewModel
{
    public UserProfileResponse User { get; set; } = new();
}