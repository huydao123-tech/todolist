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

public partial class CalendarViewModel : ObservableObject
{
    private readonly ITaskService _taskService;

    [ObservableProperty]
    private DateTime _currentWeekDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<DayGroupViewModel> _agendaDays = new();

    /// <summary>
    /// Dùng cho dải 7 ngày nằm ngang ở trên cùng (Mon -> Sun).
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DayGroupViewModel> _topWeekDays = new();

    public string MonthRangeDisplay
    {
        get
        {
            var startDate = CurrentWeekDate.Date;
            var endDate = startDate.AddDays(6);
            if (startDate.Month == endDate.Month)
                return $"{startDate.ToString("MMMM yyyy")}";
            else
                return $"{startDate.ToString("MMM")} - {endDate.ToString("MMM yyyy")}";
        }
    }

    public CalendarViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        _ = LoadWeekAsync();
    }

    public async Task LoadWeekAsync()
    {
        try
        {
            if (CurrentWeekDate.Date < DateTime.Today)
            {
                CurrentWeekDate = DateTime.Today;
            }
            var startDate = CurrentWeekDate.Date;
            // Lấy 14 ngày cho danh sách dọc (2 tuần)
            var endOfAgenda = startDate.AddDays(13);

            var tasks = await _taskService.GetTasksByDateRangeAsync(App.CurrentUser?.Id ?? App.DefaultUserId, startDate, endOfAgenda.AddDays(1).AddTicks(-1));

            var topDays = new ObservableCollection<DayGroupViewModel>();
            var agenda = new ObservableCollection<DayGroupViewModel>();

            for (int i = 0; i < 14; i++)
            {
                var day = startDate.AddDays(i);
                var dayTasks = tasks
                    .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == day.Date)
                    .OrderBy(t => t.DueDate)
                    .ToList();

                var dayGroup = new DayGroupViewModel
                {
                    Date = day,
                    Tasks = new ObservableCollection<TaskItem>(dayTasks)
                };

                agenda.Add(dayGroup);
                
                // 7 ngày đầu đưa vào Top bar
                if (i < 7)
                {
                    topDays.Add(dayGroup);
                }
            }

            TopWeekDays = topDays;
            AgendaDays = agenda;
            OnPropertyChanged(nameof(MonthRangeDisplay));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi khi tải dữ liệu lịch:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task NextWeekAsync()
    {
        CurrentWeekDate = CurrentWeekDate.AddDays(7);
        await LoadWeekAsync();
    }

    [RelayCommand]
    private async Task PreviousWeekAsync()
    {
        var newDate = CurrentWeekDate.AddDays(-7);
        if (newDate.Date < DateTime.Today)
        {
            newDate = DateTime.Today;
        }
        CurrentWeekDate = newDate;
        await LoadWeekAsync();
    }

    [RelayCommand]
    private async Task GoToTodayAsync()
    {
        CurrentWeekDate = DateTime.Today;
        await LoadWeekAsync();
    }

    [RelayCommand]
    private async Task ShowTaskDetailsAsync(TaskItem task)
    {
        if (task == null) return;
        var dialogVM = new TaskDetailDialogViewModel(task, _taskService);
        await MaterialDesignThemes.Wpf.DialogHost.Show(dialogVM, "MainDialogHost");
    }

    [RelayCommand]
    private void AddTaskForDate(DateTime date)
    {
        // Chuyển hướng sang trang Create Task
        WeakReferenceMessenger.Default.Send(new NavigationMessage("CreateTask"));
    }
}
