using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class MindSandboxViewModel : ObservableObject, IRecipient<TaskDataChangedMessage>
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private string _newIdeaText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _ideas = new();

    public MindSandboxViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        WeakReferenceMessenger.Default.Register<TaskDataChangedMessage>(this);
    }

    public async Task LoadIdeasAsync()
    {
        var allTasks = await _taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
        
        // Chỉ lấy những công việc nằm trong Inbox và chưa bị xóa
        var inboxTasks = allTasks
            .Where(t => t.Quadrant == EisenhowerQuadrant.Inbox && !t.DeletedAt.HasValue)
            .OrderByDescending(t => t.UpdatedAt)
            .ToList();

        Ideas.Clear();
        foreach (var task in inboxTasks)
        {
            Ideas.Add(task);
        }
    }

    [RelayCommand]
    private async Task CaptureIdeaAsync()
    {
        if (string.IsNullOrWhiteSpace(NewIdeaText))
            return;

        var idea = new TaskItem
        {
            UserId = App.CurrentUser?.Id ?? App.DefaultUserId,
            Title = NewIdeaText.Trim(),
            Quadrant = EisenhowerQuadrant.Inbox,
            Status = "TODO"
        };

        await _taskService.AddTaskAsync(idea);
        
        NewIdeaText = string.Empty;
        
        await LoadIdeasAsync();
        WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage());
    }

    [RelayCommand]
    private async Task DeleteIdeaAsync(TaskItem idea)
    {
        if (idea == null) return;
        
        await _taskService.DeleteTaskAsync(idea.Id);
        await LoadIdeasAsync();
        WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage());
    }

    [RelayCommand]
    private void PromoteToTask(TaskItem idea)
    {
        if (idea == null) return;
        
        // Mở Dialog chỉnh sửa Task
        var dialogVM = new TaskDetailDialogViewModel(idea, _taskService);
        _ = MaterialDesignThemes.Wpf.DialogHost.Show(dialogVM, "MainDialogHost");
    }

    public async void Receive(TaskDataChangedMessage message)
    {
        await LoadIdeasAsync();
    }
}
