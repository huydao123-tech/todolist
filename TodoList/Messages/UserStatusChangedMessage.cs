using CommunityToolkit.Mvvm.Messaging.Messages;
using TodoList.Models;

namespace TodoList.Messages;

public class UserStatusChangedMessage : ValueChangedMessage<User?>
{
    public UserStatusChangedMessage(User? value) : base(value)
    {
    }
}
