using CommunityToolkit.Mvvm.Messaging.Messages;

namespace TodoList.Messages;

/// <summary>
/// Message dùng để báo hiệu yêu cầu chuyển hướng màn hình.
/// Chuỗi string sẽ là tên của màn hình đích (ví dụ: "Dashboard").
/// </summary>
public class NavigationMessage : ValueChangedMessage<string>
{
    public object? Parameter { get; }

    public NavigationMessage(string targetView, object? parameter = null) : base(targetView)
    {
        Parameter = parameter;
    }
}
