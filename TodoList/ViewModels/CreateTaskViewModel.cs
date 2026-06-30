using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodoList.Messages;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class CreateTaskViewModel : ObservableObject
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private string _taskTitle = string.Empty;

    [ObservableProperty]
    private string? _taskDescription;

    public TaskItem? EditingTask { get; private set; }

    [ObservableProperty]
    private string _pageTitle = "Create New Task";

    [ObservableProperty]
    private string _pageSubtitle = "Fill in the details for your new task.";

    [ObservableProperty]
    private string _submitButtonText = "Save Task";

    /// <summary>
    /// Thứ trong tuần mà người dùng chọn để làm task này (0=T2, 1=T3, ..., 6=CN).
    /// </summary>
    [ObservableProperty]
    private int _selectedDayOfWeek = (int)DateTime.Today.DayOfWeek == 0
        ? 6  // Chủ nhật → index 6
        : (int)DateTime.Today.DayOfWeek - 1; // T2=0, T3=1, ..., T7=5

    /// <summary>
    /// Giờ bắt đầu (0–23), mặc định 9 giờ sáng.
    /// </summary>
    [ObservableProperty]
    private int _selectedHour = 9;

    /// <summary>
    /// Phút bắt đầu (0 hoặc 30).
    /// </summary>
    [ObservableProperty]
    private int _selectedMinute = 0;

    /// <summary>
    /// Giờ kết thúc (0–23), mặc định 10 giờ.
    /// </summary>
    [ObservableProperty]
    private int _selectedEndHour = 10;

    /// <summary>
    /// Phút kết thúc (0, 15, 30, 45).
    /// </summary>
    [ObservableProperty]
    private int _selectedEndMinute = 0;

    /// <summary>
    /// Mức độ ưu tiên: High, Medium, Low.
    /// </summary>
    [ObservableProperty]
    private string _selectedPriority = "Medium";

    /// <summary>
    /// Danh sách các thứ trong tuần để hiển thị ComboBox.
    /// </summary>
    public ObservableCollection<DayOfWeekItem> DaysOfWeek { get; } = new()
    {
        new DayOfWeekItem { Index = 0, Name = "Monday (Thứ Hai)" },
        new DayOfWeekItem { Index = 1, Name = "Tuesday (Thứ Ba)" },
        new DayOfWeekItem { Index = 2, Name = "Wednesday (Thứ Tư)" },
        new DayOfWeekItem { Index = 3, Name = "Thursday (Thứ Năm)" },
        new DayOfWeekItem { Index = 4, Name = "Friday (Thứ Sáu)" },
        new DayOfWeekItem { Index = 5, Name = "Saturday (Thứ Bảy)" },
        new DayOfWeekItem { Index = 6, Name = "Sunday (Chủ Nhật)" },
    };

    /// <summary>
    /// Danh sách giờ (0–23).
    /// </summary>
    public ObservableCollection<int> Hours { get; } = new(Enumerable.Range(0, 24));

    /// <summary>
    /// Danh sách phút (00, 15, 30, 45).
    /// </summary>
    public ObservableCollection<int> Minutes { get; } = new() { 0, 15, 30, 45 };

    /// <summary>
    /// Danh sách mức ưu tiên.
    /// </summary>
    public ObservableCollection<string> Priorities { get; } = new() { "High", "Medium", "Low" };

    public CreateTaskViewModel(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public void LoadTask(TaskItem? task)
    {
        EditingTask = task;
        if (task != null)
        {
            PageTitle = "Edit Task";
            PageSubtitle = "Update the details of your task.";
            SubmitButtonText = "Update Task";

            TaskTitle = task.Title;
            TaskDescription = task.Description;
            SelectedPriority = task.Priority;

            if (task.DueDate.HasValue)
            {
                var due = task.DueDate.Value;
                SelectedDayOfWeek = (int)due.DayOfWeek == 0 ? 6 : (int)due.DayOfWeek - 1;
                SelectedHour = due.Hour;
                SelectedMinute = due.Minute;
            }

            if (task.EndTime.HasValue)
            {
                SelectedEndHour = task.EndTime.Value.Hour;
                SelectedEndMinute = task.EndTime.Value.Minute;
            }
        }
        else
        {
            PageTitle = "Create New Task";
            PageSubtitle = "Fill in the details for your new task.";
            SubmitButtonText = "Save Task";

            TaskTitle = string.Empty;
            TaskDescription = string.Empty;
            SelectedDayOfWeek = (int)DateTime.Today.DayOfWeek == 0 ? 6 : (int)DateTime.Today.DayOfWeek - 1;
            SelectedHour = 9;
            SelectedMinute = 0;
            SelectedEndHour = 10;
            SelectedEndMinute = 0;
            SelectedPriority = "Medium";
        }
    }

    /// <summary>
    /// Tính ngày cụ thể trong tuần hiện tại dựa trên thứ người dùng chọn.
    /// Index 0 = Thứ Hai, ..., Index 6 = Chủ Nhật.
    /// </summary>
    private DateTime GetDateForSelectedDay()
    {
        // Tính ngày Thứ Hai của tuần hiện tại
        var today = DateTime.Today;
        int diffToMonday = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var monday = today.AddDays(-diffToMonday);

        // Lấy ngày tương ứng với thứ được chọn
        var selectedDate = monday.AddDays(SelectedDayOfWeek);

        // Kết hợp với giờ và phút
        return selectedDate.AddHours(SelectedHour).AddMinutes(SelectedMinute);
    }

    /// <summary>
    /// Tính ngày giờ kết thúc dựa trên thứ người dùng chọn.
    /// </summary>
    private DateTime GetEndDateForSelectedDay()
    {
        var today = DateTime.Today;
        int diffToMonday = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
        var monday = today.AddDays(-diffToMonday);
        var selectedDate = monday.AddDays(SelectedDayOfWeek);
        return selectedDate.AddHours(SelectedEndHour).AddMinutes(SelectedEndMinute);
    }

    /// <summary>
    /// Lưu công việc mới vào cơ sở dữ liệu và quay lại Dashboard.
    /// </summary>
    [RelayCommand]
    private async Task SaveTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(TaskTitle))
        {
            MessageBox.Show("Vui lòng nhập tiêu đề công việc.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dueDate = GetDateForSelectedDay();
        var endTime = GetEndDateForSelectedDay();

        if (endTime <= dueDate)
        {
            MessageBox.Show("Giờ kết thúc phải sau giờ bắt đầu.", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (EditingTask != null)
        {
            EditingTask.Title = TaskTitle;
            EditingTask.Description = TaskDescription;
            EditingTask.DueDate = dueDate;
            EditingTask.EndTime = endTime;
            EditingTask.Priority = SelectedPriority;
            EditingTask.IsImportant = SelectedPriority == "High";
            EditingTask.IsUrgent = SelectedPriority == "High";

            try
            {
                await _taskService.UpdateTaskAsync(EditingTask);
                LoadTask(null); // Reset form
                WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage());
                WeakReferenceMessenger.Default.Send(new NavigationMessage("Dashboard"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể cập nhật công việc:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            var newTask = new TaskItem
            {
                UserId      = App.CurrentUser?.Id ?? App.DefaultUserId,
                Title       = TaskTitle,
                Description = TaskDescription,
                DueDate     = dueDate,
                EndTime     = endTime,
                Priority    = SelectedPriority,
                IsImportant = SelectedPriority == "High",
                IsUrgent    = SelectedPriority == "High",
                Status      = "TODO"
            };

            try
            {
                await _taskService.AddTaskAsync(newTask);
                LoadTask(null); // Reset form
                WeakReferenceMessenger.Default.Send(new TaskDataChangedMessage());
                WeakReferenceMessenger.Default.Send(new NavigationMessage("Dashboard"));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tạo công việc:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Hủy bỏ việc tạo công việc và quay lại Dashboard.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        LoadTask(null); // Reset before leaving
        WeakReferenceMessenger.Default.Send(new NavigationMessage("Dashboard"));
    }
}

/// <summary>
/// Model phụ trợ biểu diễn một thứ trong tuần cho ComboBox.
/// </summary>
public class DayOfWeekItem
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
}
