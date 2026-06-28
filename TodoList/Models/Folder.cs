using System;
using System.Collections.Generic;

namespace TodoList.Models;

public class Folder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TodoListList> Lists { get; set; } = new List<TodoListList>();
}
