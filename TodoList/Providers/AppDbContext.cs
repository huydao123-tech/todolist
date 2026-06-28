using Microsoft.EntityFrameworkCore;
using TodoList.Models;

namespace TodoList.Providers;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Folder> Folders { get; set; } = null!;
    public DbSet<TodoListList> Lists { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TaskTag> TaskTags { get; set; } = null!;
    public DbSet<PomodoroSession> PomodoroSessions { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity => {
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Folder>(entity => {
            entity.ToTable("folders");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasOne(d => d.User).WithMany(p => p.Folders).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TodoListList>(entity => {
            entity.ToTable("lists");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FolderId).HasColumnName("folder_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ColorCode).HasColumnName("color_code");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasOne(d => d.User).WithMany(p => p.Lists).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Folder).WithMany(p => p.Lists).HasForeignKey(d => d.FolderId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskItem>(entity => {
            entity.ToTable("tasks");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.ListId).HasColumnName("list_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsImportant).HasColumnName("is_important");
            entity.Property(e => e.IsUrgent).HasColumnName("is_urgent");
            entity.Property(e => e.DueDate).HasColumnName("due_date");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Ignore(e => e.TimeRangeDisplay);
            entity.Property(e => e.ReminderTime).HasColumnName("reminder_time");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Priority).HasColumnName("priority").HasDefaultValue("Medium");
            entity.Property(e => e.GoogleEventId).HasColumnName("google_event_id");
            entity.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Quadrant).HasColumnName("eisenhower_quadrant").HasDefaultValue(EisenhowerQuadrant.Inbox);

            entity.HasOne(d => d.User).WithMany(p => p.Tasks).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.List).WithMany(p => p.Tasks).HasForeignKey(d => d.ListId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(d => d.Parent).WithMany(p => p.Subtasks).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tag>(entity => {
            entity.ToTable("tags");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ColorCode).HasColumnName("color_code");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasOne(d => d.User).WithMany(p => p.Tags).HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TaskTag>(entity => {
            entity.ToTable("task_tags");
            entity.HasKey(e => new { e.TaskId, e.TagId });
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.TagId).HasColumnName("tag_id");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskTags).HasForeignKey(d => d.TaskId);
            entity.HasOne(d => d.Tag).WithMany(p => p.TaskTags).HasForeignKey(d => d.TagId);
        });

        modelBuilder.Entity<PomodoroSession>(entity => {
            entity.ToTable("pomodoro_sessions");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.IsCompleted).HasColumnName("is_completed");

            entity.HasOne(d => d.Task).WithMany(p => p.PomodoroSessions).HasForeignKey(d => d.TaskId);
        });
    }
}
