namespace AIStudyPlanner.Api.Entities;

public enum GoalStatus
{
    Draft,
    Active,
    Completed,
    Archived
}

public enum GoalPriority
{
    Low,
    Medium,
    High
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public enum PreferredStudyTime
{
    Morning,
    Afternoon,
    Evening,
    Flexible
}

public enum BreakPreference
{
    Pomodoro,
    ShortBreaks,
    LongBreaks,
    Minimal
}

public enum TaskType
{
    Learn,
    Revise,
    Practice,
    Test
}

public enum ReminderChannel
{
    InApp,
    Email,
    BrowserPush
}

public enum ReminderDeliveryStatus
{
    Pending,
    Sent,
    Failed
}

public enum NotificationType
{
    Reminder,
    StudyInsight,
    System
}

public enum AiRequestStatus
{
    Success,
    Failed,
    Fallback
}
