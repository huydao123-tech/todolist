using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly IGoogleCalendarSyncService _syncService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _todayTasksList = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> _allTasksList = new();

    [ObservableProperty]
    private int _selectedTabIndex = 0;

    [ObservableProperty]
    private string _titleText = "Today's Focus";

    [ObservableProperty]
    private string _subTitleText = "";

    [ObservableProperty]
    private string _newTaskTitle = string.Empty;

    partial void OnSelectedTabIndexChanged(int value)
    {
        UpdateDisplayedTasks();
    }

    private bool _isDataLoaded;
    private readonly SemaphoreSlim _loadLock = new(1, 1);

    public void InvalidateCache()
    {
        _isDataLoaded = false;
    }

    public DashboardViewModel(ITaskService taskService, IGoogleCalendarSyncService syncService, IServiceProvider serviceProvider)
    {
        _taskService = taskService;
        _syncService = syncService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Tải danh sách công việc từ Service và cập nhật lên giao diện.
    /// </summary>
    public async Task LoadTasksAsync(bool forceReload = true)
    {
        if (!forceReload && _isDataLoaded) return;
        await _loadLock.WaitAsync();
        try
        {
            if (!forceReload && _isDataLoaded) return;

            var tasksList = await _taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
            var activeTasks = tasksList.Where(t => !t.DeletedAt.HasValue).ToList();
            
            var todayItems = activeTasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today).ToList();
            var allItems = activeTasks
                .GroupBy(t => t.Title.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(t => t.DueDate ?? DateTime.MinValue).First())
                .ToList();

            TodayTasksList = new ObservableCollection<TaskItem>(todayItems);
            AllTasksList = new ObservableCollection<TaskItem>(allItems);
            
            _isDataLoaded = true;
            UpdateDisplayedTasks();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private void UpdateDisplayedTasks()
    {
        if (SelectedTabIndex == 0)
        {
            TitleText = "Today's Focus";
            SubTitleText = $"{DateTime.Today:dddd, MMM d} — You have {TodayTasksList.Count(t => t.Status != "DONE")} tasks remaining.";
        }
        else
        {
            TitleText = "All Tasks";
            SubTitleText = $"You have a total of {AllTasksList.Count} tasks.";
        }
    }

    /// <summary>
    /// Lệnh (Command) thêm nhanh một công việc mới.
    /// </summary>
    [RelayCommand]
    private async Task AddQuickTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

        var newTask = new TaskItem
        {
            UserId = App.CurrentUser?.Id ?? App.DefaultUserId,
            Title = NewTaskTitle,
            IsImportant = false,
            IsUrgent = false,
            DueDate = DateTime.Today,
            Status = "TODO"
        };

        try
        {
            var addedTask = await _taskService.AddTaskAsync(newTask);
            await LoadTasksAsync(); // Nạp lại dữ liệu để cập nhật đúng nhóm và danh sách các tab
            NewTaskTitle = string.Empty;
            WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage(this));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể thêm công việc:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task ShowTaskDetailsAsync(TaskItem task)
    {
        if (task == null) return;
        var dialogVM = new TaskDetailDialogViewModel(task, _taskService);
        await MaterialDesignThemes.Wpf.DialogHost.Show(dialogVM, "MainDialogHost");
    }

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItem task)
    {
        if (task == null) return;
        
        // Nếu ở tab All Tasks (SelectedTabIndex == 1), hỏi người dùng xóa tất cả các task trùng tên
        string confirmMsg = SelectedTabIndex == 1 
            ? $"Bạn có chắc muốn xóa tất cả các bản ghi (kể cả lịch ngày khác) của công việc '{task.Title}' không?"
            : $"Bạn có chắc muốn xóa công việc '{task.Title}'?";

        var result = MessageBox.Show(confirmMsg, "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                if (SelectedTabIndex == 1)
                {
                    // Lấy lại danh sách thực tế từ DB để tìm các task cùng tên
                    var tasksList = await _taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
                    var toDelete = tasksList.Where(t => !t.DeletedAt.HasValue && string.Equals(t.Title.Trim(), task.Title.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    foreach (var t in toDelete)
                    {
                        await _taskService.DeleteTaskAsync(t.Id);
                    }
                }
                else
                {
                    // Ở tab Today, chỉ xóa chính xác task này
                    await _taskService.DeleteTaskAsync(task.Id);
                }
                
                await LoadTasksAsync(); // Tải lại dữ liệu để đồng bộ cả 2 danh sách
                WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage(this));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể xóa công việc:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task SyncGoogleCalendarAsync()
    {
        try
        {
            await _syncService.PullTasksAsync(_serviceProvider);
            await LoadTasksAsync(); // Refresh UI
            WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage(this));
            MessageBox.Show("Đồng bộ Google Calendar thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi đồng bộ Google Calendar:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
