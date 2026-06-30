using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class TaDaListViewModel : ObservableObject, IRecipient<TaskDataChangedMessage>
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _completedTasks = new();

    [ObservableProperty]
    private int _completedCount;

    [ObservableProperty]
    private string _encouragingMessage = string.Empty;

    public TaDaListViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        WeakReferenceMessenger.Default.Register<TaskDataChangedMessage>(this);
    }

    public async Task LoadDataAsync()
    {
        var allTasks = await _taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
        
        // Lọc ra các công việc ĐÃ XONG và có ngày UpdateAt là HÔM NAY
        var today = DateTime.UtcNow.Date;
        var tadaTasks = allTasks
            .Where(t => t.Status == "DONE" && t.UpdatedAt.Date == today)
            .OrderByDescending(t => t.UpdatedAt)
            .ToList();

        CompletedTasks.Clear();
        foreach (var task in tadaTasks)
        {
            CompletedTasks.Add(task);
        }

        CompletedCount = CompletedTasks.Count;
        UpdateEncouragingMessage();
    }

    private void UpdateEncouragingMessage()
    {
        if (CompletedCount == 0)
        {
            EncouragingMessage = "Chưa có việc nào xong. Nhớ rằng nghỉ ngơi cũng là một loại công việc!";
        }
        else if (CompletedCount >= 1 && CompletedCount <= 3)
        {
            EncouragingMessage = "Tuyệt vời! Một khởi đầu quá tốt cho hôm nay!";
        }
        else if (CompletedCount >= 4 && CompletedCount <= 6)
        {
            EncouragingMessage = "Thật điên rồ! Bạn đang có một ngày siêu năng suất!";
        }
        else
        {
            EncouragingMessage = "Vô đối! Kỷ lục Guinness cần ghi tên bạn ngay lập tức! 🚀";
        }
    }

    public async void Receive(TaskDataChangedMessage message)
    {
        await LoadDataAsync();
    }
}
