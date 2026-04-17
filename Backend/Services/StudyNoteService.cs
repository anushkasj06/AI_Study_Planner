using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Assistant;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class StudyNoteService : IStudyNoteService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IAssistantComposerService assistantComposerService;

    public StudyNoteService(ApplicationDbContext dbContext, IAssistantComposerService assistantComposerService)
    {
        this.assistantComposerService = assistantComposerService;
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<StudyNoteResponse>> GetNotesAsync(Guid userId)
    {
        var notes = await dbContext.StudyNotes
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.UpdatedAt)
            .Take(100)
            .ToListAsync();

        return notes.Select(Map).ToList();
    }

    public async Task<StudyNoteResponse> CreateFromAssistantAsync(
        Guid userId,
        CreateNoteFromAssistantRequest request,
        CancellationToken cancellationToken = default)
    {
        StudyGoal? goal = null;
        if (request.StudyGoalId.HasValue)
        {
            goal = await dbContext.StudyGoals
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == request.StudyGoalId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Goal not found.");
        }

        var draft = await assistantComposerService.CreateNoteDraftAsync(userId, request.StudyGoalId, request.Prompt, cancellationToken);

        var note = new StudyNote
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StudyGoalId = goal?.Id,
            Title = draft.Title,
            ContentMarkdown = draft.ContentMarkdown,
            MindMapMermaid = draft.MindMapMermaid,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.StudyNotes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(note);
    }

    public async Task DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default)
    {
        var note = await dbContext.StudyNotes
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == noteId, cancellationToken)
            ?? throw new KeyNotFoundException("Note not found.");

        dbContext.StudyNotes.Remove(note);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static StudyNoteResponse Map(StudyNote note) => new()
    {
        Id = note.Id,
        StudyGoalId = note.StudyGoalId,
        Title = note.Title,
        ContentMarkdown = note.ContentMarkdown,
        MindMapMermaid = note.MindMapMermaid,
        CreatedAt = note.CreatedAt,
        UpdatedAt = note.UpdatedAt
    };
}
