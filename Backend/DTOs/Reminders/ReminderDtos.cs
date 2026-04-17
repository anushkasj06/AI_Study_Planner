using AIStudyPlanner.Api.Entities;
using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Reminders;

public class ReminderCreateRequest
{
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public ReminderChannel Channel { get; set; } = ReminderChannel.InApp;
}

public class ReminderBatchCreateRequest
{
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public List<ReminderChannel> Channels { get; set; } = [];
}

public class AiReminderDraftRequest
{
    public Guid? StudyTaskId { get; set; }
    public Guid? StudyGoalId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public DateTime? PreferredReminderDateTime { get; set; }
    public bool PreferEmail { get; set; }
    public bool PreferBrowserPush { get; set; }
}

public class AiReminderDraftResponse
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public IReadOnlyCollection<ReminderChannel> RecommendedChannels { get; set; } = [];
    public string Reasoning { get; set; } = string.Empty;
}

public class ReminderPipelineActionResponse
{
    public int Count { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ReminderUpdateRequest : ReminderCreateRequest
{
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
}

public class ReminderResponse
{
    public Guid Id { get; set; }
    public Guid? StudyTaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ReminderDateTime { get; set; }
    public bool IsSent { get; set; }
    public bool IsRead { get; set; }
    public ReminderChannel Channel { get; set; }
    public ReminderDeliveryStatus DeliveryStatus { get; set; }
    public int DeliveryAttempts { get; set; }
    public DateTime? LastDeliveryAttemptAtUtc { get; set; }
    public string LastDeliveryError { get; set; } = string.Empty;
}

public class ReminderCreateRequestValidator : FluentValidation.AbstractValidator<ReminderCreateRequest>
{
    public ReminderCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ReminderDateTime)
            .Must(x => x > DateTime.UtcNow.AddMinutes(-2))
            .WithMessage("Reminder time should be now or in the future.");
    }
}

public class ReminderBatchCreateRequestValidator : FluentValidation.AbstractValidator<ReminderBatchCreateRequest>
{
    public ReminderBatchCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ReminderDateTime)
            .Must(x => x > DateTime.UtcNow.AddMinutes(-2))
            .WithMessage("Reminder time should be now or in the future.");
        RuleFor(x => x.Channels)
            .NotEmpty()
            .WithMessage("Choose at least one delivery channel.");
    }
}

public class AiReminderDraftRequestValidator : FluentValidation.AbstractValidator<AiReminderDraftRequest>
{
    public AiReminderDraftRequestValidator()
    {
        RuleFor(x => x.Prompt).MaximumLength(600);
    }
}

public class ReminderUpdateRequestValidator : FluentValidation.AbstractValidator<ReminderUpdateRequest>
{
    public ReminderUpdateRequestValidator()
    {
        Include(new ReminderCreateRequestValidator());
    }
}
