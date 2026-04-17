using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Assistant;

public class AssistantChatRequest
{
    public string Message { get; set; } = string.Empty;
}

public class AssistantChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<AssistantSuggestion> Suggestions { get; set; } = [];
    public string MindMapMermaid { get; set; } = string.Empty;
    public string NoteTitle { get; set; } = string.Empty;
    public string NoteMarkdown { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool UsedFallback { get; set; }
}

public class AssistantSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Payload { get; set; }
}

public class CreateNoteFromAssistantRequest
{
    public Guid? StudyGoalId { get; set; }
    public string Prompt { get; set; } = string.Empty;
}

public class AssistantNoteDraftResponse
{
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public string MindMapMermaid { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public bool UsedFallback { get; set; }
}

public class StudyNoteResponse
{
    public Guid Id { get; set; }
    public Guid? StudyGoalId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentMarkdown { get; set; } = string.Empty;
    public string MindMapMermaid { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AssistantChatRequestValidator : AbstractValidator<AssistantChatRequest>
{
    public AssistantChatRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1500);
    }
}

public class CreateNoteFromAssistantRequestValidator : AbstractValidator<CreateNoteFromAssistantRequest>
{
    public CreateNoteFromAssistantRequestValidator()
    {
        RuleFor(x => x.Prompt).NotEmpty().MaximumLength(2000);
    }
}
