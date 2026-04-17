using AIStudyPlanner.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<StudyGoal> StudyGoals => Set<StudyGoal>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<StudyTask> StudyTasks => Set<StudyTask>();
    public DbSet<ProgressLog> ProgressLogs => Set<ProgressLog>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<AiRequestLog> AiRequestLogs => Set<AiRequestLog>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<WebPushSubscription> WebPushSubscriptions => Set<WebPushSubscription>();
    public DbSet<StudyNote> StudyNotes => Set<StudyNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();
            entity.HasIndex(x => x.PhoneNumber).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(120);
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.PhoneNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<StudyGoal>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Status });
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(1500);
            entity.Property(x => x.DailyAvailableHours).HasPrecision(5, 2);
            entity.Property(x => x.DifficultyLevel).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.PreferredStudyTime).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.BreakPreference).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(x => x.User)
                .WithMany(x => x.StudyGoals)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudyPlan>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.StudyGoalId });
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.TotalEstimatedHours).HasPrecision(8, 2);

            entity.HasOne(x => x.User)
                .WithMany(x => x.StudyPlans)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyGoal)
                .WithMany(x => x.StudyPlans)
                .HasForeignKey(x => x.StudyGoalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudyTask>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.TaskDate });
            entity.HasIndex(x => new { x.StudyGoalId, x.IsCompleted });
            entity.Property(x => x.Topic).HasMaxLength(150);
            entity.Property(x => x.Subtopic).HasMaxLength(150);
            entity.Property(x => x.EstimatedHours).HasPrecision(5, 2);
            entity.Property(x => x.ActualHours).HasPrecision(5, 2);
            entity.Property(x => x.TaskType).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Notes).HasMaxLength(1200);

            entity.HasOne(x => x.StudyPlan)
                .WithMany(x => x.Tasks)
                .HasForeignKey(x => x.StudyPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyGoal)
                .WithMany(x => x.StudyTasks)
                .HasForeignKey(x => x.StudyGoalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(x => x.StudyTasks)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProgressLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.Date });
            entity.Property(x => x.HoursSpent).HasPrecision(5, 2);
            entity.Property(x => x.Notes).HasMaxLength(1000);

            entity.HasOne(x => x.User)
                .WithMany(x => x.ProgressLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyTask)
                .WithMany(x => x.ProgressLogs)
                .HasForeignKey(x => x.StudyTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyGoal)
                .WithMany(x => x.ProgressLogs)
                .HasForeignKey(x => x.StudyGoalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.ReminderDateTime });
            entity.HasIndex(x => new { x.IsSent, x.ReminderDateTime });
            entity.HasIndex(x => new { x.DeliveryStatus, x.ReminderDateTime });
            entity.Property(x => x.Title).HasMaxLength(150);
            entity.Property(x => x.Message).HasMaxLength(500);
            entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.DeliveryStatus).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.LastDeliveryError).HasMaxLength(800);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyTask)
                .WithMany(x => x.Reminders)
                .HasForeignKey(x => x.StudyTaskId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.HasIndex(x => new { x.UserId, x.IsRead });
            entity.Property(x => x.Title).HasMaxLength(150);
            entity.Property(x => x.Message).HasMaxLength(500);
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Notifications)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Reminder)
                .WithMany()
                .HasForeignKey(x => x.ReminderId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<WebPushSubscription>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Endpoint).IsUnique();
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Endpoint).HasMaxLength(700);
            entity.Property(x => x.P256dh).HasMaxLength(300);
            entity.Property(x => x.Auth).HasMaxLength(300);

            entity.HasOne(x => x.User)
                .WithMany(x => x.WebPushSubscriptions)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<StudyNote>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.Property(x => x.Title).HasMaxLength(180);
            entity.Property(x => x.ContentMarkdown).HasColumnType("longtext");
            entity.Property(x => x.MindMapMermaid).HasColumnType("longtext");

            entity.HasOne(x => x.User)
                .WithMany(x => x.StudyNotes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyGoal)
                .WithMany()
                .HasForeignKey(x => x.StudyGoalId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AiRequestLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.CreatedAt });
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            entity.HasOne(x => x.User)
                .WithMany(x => x.AiRequestLogs)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.StudyGoal)
                .WithMany(x => x.AiRequestLogs)
                .HasForeignKey(x => x.StudyGoalId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
