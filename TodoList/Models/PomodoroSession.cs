using System;

namespace TodoList.Models;

public class PomodoroSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TaskId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public int Duration { get; set; } // minutes
    public bool IsCompleted { get; set; } = false;

    public TaskItem Task { get; set; } = null!;
}
