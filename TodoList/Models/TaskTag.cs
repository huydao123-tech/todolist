using System;

namespace TodoList.Models;

public class TaskTag
{
    public Guid TaskId { get; set; }
    public Guid TagId { get; set; }

    public TaskItem Task { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
