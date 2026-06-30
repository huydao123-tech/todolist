using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TodoList.Messages;

/// <summary>
/// Message dùng để báo hiệu dữ liệu danh sách công việc đã thay đổi.
/// </summary>
public class TaskDataChangedMessage
{
    public TaskDataChangedMessage(object? source = null)
    {
        Source = source;
    }

    public object? Source { get; }
}
