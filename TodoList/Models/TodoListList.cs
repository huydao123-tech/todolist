using System;
using System.Collections.Generic;

namespace TodoList.Models;

public class TodoListList
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? FolderId { get; set; }
    public string Name { get; set; } = null!;
    public string? ColorCode { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
    public Folder? Folder { get; set; }
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
