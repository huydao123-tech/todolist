using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ITaskService _taskService;
    private readonly IGoogleCalendarSyncService _syncService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _displayedTasks = new();

    private List<TaskItem> _todayTasksList = new();
    private List<TaskItem> _allTasksList = new();

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

    public DashboardViewModel(ITaskService taskService, IGoogleCalendarSyncService syncService, IServiceProvider serviceProvider)
    {
        _taskService = taskService;
        _syncService = syncService;
        _serviceProvider = serviceProvider;
        _ = LoadTasksAsync();
    }

    /// <summary>
    /// Tải danh sách công việc từ Service và cập nhật lên giao diện.
    /// </summary>
    public async Task LoadTasksAsync()
    {
        try
        {
            var tasksList = await _taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
            
            // Lọc bỏ các công việc đã bị xóa mềm (soft-deleted)
            var activeTasks = tasksList.Where(t => !t.DeletedAt.HasValue).ToList();
            
            // Danh sách Today: Lấy các công việc của ngày hôm nay (giữ nguyên từng ngày riêng biệt)
            _todayTasksList = activeTasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == DateTime.Today).ToList();
            
            // Danh sách All Tasks: Gộp nhóm theo Tiêu đề (không phân biệt ngày tháng) để loại bỏ các task trùng lặp hoàn toàn
            _allTasksList = activeTasks
                .GroupBy(t => t.Title.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(t => t.DueDate ?? DateTime.MinValue).First())
                .ToList();
            
            UpdateDisplayedTasks();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu:\n{ex.Message}\n\n{ex.InnerException?.Message}",
                "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateDisplayedTasks()
    {
        if (SelectedTabIndex == 0)
        {
            DisplayedTasks = new ObservableCollection<TaskItem>(_todayTasksList);
            TitleText = "Today's Focus";
            SubTitleText = $"{DateTime.Today:dddd, MMM d} — You have {_todayTasksList.Count(t => t.Status != "DONE")} tasks remaining.";
        }
        else
        {
            DisplayedTasks = new ObservableCollection<TaskItem>(_allTasksList);
            TitleText = "All Tasks";
            SubTitleText = $"You have a total of {_allTasksList.Count} tasks.";
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
            MessageBox.Show("Đồng bộ Google Calendar thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi đồng bộ Google Calendar:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
