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
}

public class ReminderCreateRequestValidator : FluentValidation.AbstractValidator<ReminderCreateRequest>
{
    public ReminderCreateRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(500);
    }
}

public class ReminderUpdateRequestValidator : FluentValidation.AbstractValidator<ReminderUpdateRequest>
{
    public ReminderUpdateRequestValidator()
    {
        Include(new ReminderCreateRequestValidator());
    }
}
