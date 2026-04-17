using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Notifications;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class NotificationService(ApplicationDbContext dbContext) : INotificationService
{
    public async Task<IReadOnlyCollection<NotificationResponse>> GetNotificationsAsync(Guid userId, bool includeRead)
    {
        var query = dbContext.UserNotifications
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .AsQueryable();

        if (!includeRead)
        {
            query = query.Where(x => !x.IsRead);
        }

        var notifications = await query.Take(100).ToListAsync();
        return notifications.Select(Map).ToList();
    }

    public async Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await dbContext.UserNotifications
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == notificationId)
            ?? throw new KeyNotFoundException("Notification not found.");

        notification.IsRead = true;
        await dbContext.SaveChangesAsync();
        return Map(notification);
    }

    public async Task<int> MarkAllReadAsync(Guid userId)
    {
        var notifications = await dbContext.UserNotifications
            .Where(x => x.UserId == userId && !x.IsRead)
            .ToListAsync();

        foreach (var item in notifications)
        {
            item.IsRead = true;
        }

        await dbContext.SaveChangesAsync();
        return notifications.Count;
    }

    public async Task<UserNotification> CreateAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? reminderId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = new UserNotification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ReminderId = reminderId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.UserNotifications.Add(notification);
        await dbContext.SaveChangesAsync(cancellationToken);
        return notification;
    }

    private static NotificationResponse Map(UserNotification notification) => new()
    {
        Id = notification.Id,
        Title = notification.Title,
        Message = notification.Message,
        Type = notification.Type,
        IsRead = notification.IsRead,
        CreatedAt = notification.CreatedAt
    };
}
