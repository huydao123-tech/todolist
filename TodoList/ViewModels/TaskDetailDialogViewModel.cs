using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MaterialDesignThemes.Wpf;
using System.Threading.Tasks;
using System.Windows;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class TaskDetailDialogViewModel : ObservableObject
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private TaskItem _taskItem;

    public TaskDetailDialogViewModel(TaskItem taskItem, ITaskService taskService)
    {
        _taskItem = taskItem;
        _taskService = taskService;
    }

    [RelayCommand]
    private void EditTask()
    {
        DialogHost.Close("MainDialogHost");
        WeakReferenceMessenger.Default.Send(new NavigationMessage("EditTask", TaskItem));
    }

    [RelayCommand]
    private async Task DeleteTaskAsync()
    {
        var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa công việc '{TaskItem.Title}'?", 
                                     "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            await _taskService.DeleteTaskAsync(TaskItem.Id);
            DialogHost.Close("MainDialogHost");
            WeakReferenceMessenger.Default.Send(new NavigationMessage("Dashboard"));
        }
    }
}
