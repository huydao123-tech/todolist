using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class SurpriseTaskDialogViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly List<TaskItem> _candidateTasks;
    private readonly Random _random = new();

    [ObservableProperty]
    private TaskItem _currentSurpriseTask = null!;

    public SurpriseTaskDialogViewModel(List<TaskItem> candidateTasks, ITaskService taskService)
    {
        _candidateTasks = candidateTasks;
        _taskService = taskService;
        
        RollNextTask();
    }

    private void RollNextTask()
    {
        if (_candidateTasks == null || _candidateTasks.Count == 0) return;
        
        int index = _random.Next(0, _candidateTasks.Count);
        CurrentSurpriseTask = _candidateTasks[index];
    }

    [RelayCommand]
    private void RollAgain()
    {
        RollNextTask();
    }

    [RelayCommand]
    private async Task MarkAsDoneAsync()
    {
        if (CurrentSurpriseTask == null) return;

        CurrentSurpriseTask.Status = "DONE";
        CurrentSurpriseTask.UpdatedAt = DateTime.UtcNow;
        
        await _taskService.UpdateTaskAsync(CurrentSurpriseTask);

        // Tính năng Lặp lại vô hạn hoặc 12 tuần sẽ được trigger nếu có logic clone, 
        // nhưng ở đây ta chỉ cần đổi Status sang DONE
        
        WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage());
        
        // Đóng Dialog
        MaterialDesignThemes.Wpf.DialogHost.CloseDialogCommand.Execute(null, null);
    }
}
