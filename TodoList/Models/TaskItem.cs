using System;
using System.Collections.Generic;

namespace TodoList.Models;

/// <summary>
/// Phân loại công việc theo Ma trận Eisenhower.
/// </summary>
public enum EisenhowerQuadrant
{
    Inbox = 0,      // Hộp thư đến / Chưa phân loại
    P1_Do = 1,      // Quan trọng & Khẩn cấp → Làm ngay
    P2_Schedule = 2, // Quan trọng & Không khẩn cấp → Lên lịch
    P3_Delegate = 3, // Không quan trọng & Khẩn cấp → Ủy quyền
    P4_Eliminate = 4 // Không quan trọng & Không khẩn cấp → Loại bỏ
}

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? ListId { get; set; }
    public Guid? ParentId { get; set; }
    
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsImportant { get; set; } = false;
    public bool IsUrgent { get; set; } = false;
    public DateTime? DueDate { get; set; }
    public DateTime? EndTime { get; set; }
    
    public string TimeRangeDisplay => $"{(DueDate.HasValue ? DueDate.Value.ToString("HH:mm") : "--:--")} - {(EndTime.HasValue ? EndTime.Value.ToString("HH:mm") : "--:--")}";
    public DateTime? ReminderTime { get; set; }
    public string Status { get; set; } = "TODO"; // TODO, IN_PROGRESS, DONE
    public string Priority { get; set; } = "Medium"; // High, Medium, Low

    /// <summary>
    /// Vị trí của công việc trong Ma trận Eisenhower.
    /// Mặc định là Inbox (0) khi mới tạo.
    /// </summary>
    public EisenhowerQuadrant Quadrant { get; set; } = EisenhowerQuadrant.Inbox;
    
    // Thuộc tính công việc lặp lại
    public bool IsRecurring { get; set; } = false;
    public string RecurrenceInterval { get; set; } = "None"; // None, Daily, Weekly, Monthly

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    
    // Google Calendar Sync Fields
    public string? GoogleEventId { get; set; }
    public DateTime? LastSyncedAt { get; set; }

    public User User { get; set; } = null!;
    public TodoListList? List { get; set; }
    
    public TaskItem? Parent { get; set; }
    public ICollection<TaskItem> Subtasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
    public ICollection<PomodoroSession> PomodoroSessions { get; set; } = new List<PomodoroSession>();
}
