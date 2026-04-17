namespace AIStudyPlanner.Api.DTOs.Dashboard;

public class DashboardSummaryResponse
{
    public int TotalGoals { get; set; }
    public int ActiveGoals { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public decimal HoursStudiedThisWeek { get; set; }
    public int StreakCount { get; set; }
    public int UnreadNotifications { get; set; }
    public IReadOnlyCollection<UpcomingReminderItem> UpcomingReminders { get; set; } = [];
    public IReadOnlyCollection<GoalProgressWidget> ProgressByGoal { get; set; } = [];
}

public class UpcomingReminderItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public bool IsRead { get; set; }
}

public class GoalProgressWidget
{
    public Guid GoalId { get; set; }
    public string GoalTitle { get; set; } = string.Empty;
    public decimal CompletionPercentage { get; set; }
}
