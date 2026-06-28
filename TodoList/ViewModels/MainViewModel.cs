using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;
using TodoList.Messages;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class MainViewModel : ObservableObject, 
    IRecipient<NavigationMessage>, 
    IRecipient<UserStatusChangedMessage>
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CalendarViewModel _calendarViewModel;
    private readonly CreateTaskViewModel _createTaskViewModel;
    private readonly EisenhowerViewModel _eisenhowerViewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleCalendarSyncService _syncService;

    [ObservableProperty]
    private ObservableObject _currentViewModel = null!;

    [ObservableProperty]
    private ObservableObject? _currentAuthViewModel;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _activePage = "Dashboard";

    [ObservableProperty]
    private string _currentUserEmail = string.Empty;

    public MainViewModel(
        DashboardViewModel dashboardViewModel, 
        CalendarViewModel calendarViewModel, 
        CreateTaskViewModel createTaskViewModel,
        EisenhowerViewModel eisenhowerViewModel,
        IServiceProvider serviceProvider,
        IGoogleCalendarSyncService syncService)
    {
        _dashboardViewModel = dashboardViewModel;
        _calendarViewModel = calendarViewModel;
        _createTaskViewModel = createTaskViewModel;
        _eisenhowerViewModel = eisenhowerViewModel;
        _serviceProvider = serviceProvider;
        _syncService = syncService;
        
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this);
        WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(this);

        ShowLogin();
    }

    /// <summary>
    /// Xử lý message điều hướng được gửi đến.
    /// </summary>
    public void Receive(NavigationMessage message)
    {
        if (message.Value == "Dashboard" || message.Value == "Today")
        {
            NavigateToToday();
        }
        else if (message.Value == "EditTask" && message.Parameter is TodoList.Models.TaskItem taskItem)
        {
            NavigateToEditTask(taskItem);
        }
        else if (message.Value == "Login")
        {
            ShowLogin();
        }
        else if (message.Value == "Register")
        {
            ShowRegister();
        }
        else if (message.Value == "Eisenhower")
        {
            NavigateToEisenhower();
        }
    }

    public void Receive(UserStatusChangedMessage message)
    {
        var user = message.Value;
        if (user != null)
        {
            CurrentUserEmail = user.Email;
            IsLoggedIn = true;
            CurrentAuthViewModel = null;
            NavigateToToday();
            // Tự động sync Google Calendar sau khi đăng nhập thành công
            _ = AutoSyncAfterLoginAsync();
        }
        else
        {
            CurrentUserEmail = string.Empty;
            IsLoggedIn = false;
            ShowLogin();
        }
    }

    private async Task AutoSyncAfterLoginAsync()
    {
        try
        {
            IsSyncing = true;
            await Task.Run(() => _syncService.PullTasksAsync(_serviceProvider));
            // Sau khi sync xong, reload lại dữ liệu trang hiện tại
            await _dashboardViewModel.LoadTasksAsync();
        }
        catch (Exception)
        {
            // Bỏ qua lỗi sync (chưa đăng nhập Google, v.v.)
        }
        finally
        {
            IsSyncing = false;
        }
    }

    private void ShowLogin()
    {
        CurrentAuthViewModel = (ObservableObject)_serviceProvider.GetService(typeof(LoginViewModel))!;
    }

    private void ShowRegister()
    {
        CurrentAuthViewModel = (ObservableObject)_serviceProvider.GetService(typeof(RegisterViewModel))!;
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Dashboard (All Tasks).
    /// </summary>
    [RelayCommand]
    private void NavigateToDashboard()
    {
        ActivePage = "Dashboard";
        _ = _dashboardViewModel.LoadTasksAsync();
        CurrentViewModel = _dashboardViewModel;
    }

    [RelayCommand]
    private void NavigateToToday()
    {
        ActivePage = "Today";
        _ = _dashboardViewModel.LoadTasksAsync();
        CurrentViewModel = _dashboardViewModel;
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Lịch (Calendar).
    /// </summary>
    [RelayCommand]
    private void NavigateToCalendar()
    {
        ActivePage = "Calendar";
        _ = _calendarViewModel.LoadWeekAsync();
        CurrentViewModel = _calendarViewModel;
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Ma trận Eisenhower.
    /// </summary>
    [RelayCommand]
    private void NavigateToEisenhower()
    {
        ActivePage = "Eisenhower";
        _ = _eisenhowerViewModel.LoadDataAsync();
        CurrentViewModel = _eisenhowerViewModel;
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Tạo công việc chi tiết.
    /// </summary>
    [RelayCommand]
    private void NavigateToCreateTask()
    {
        _createTaskViewModel.LoadTask(null);
        CurrentViewModel = _createTaskViewModel;
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Chỉnh sửa công việc.
    /// </summary>
    private void NavigateToEditTask(TodoList.Models.TaskItem task)
    {
        _createTaskViewModel.LoadTask(task);
        CurrentViewModel = _createTaskViewModel;
    }

    [RelayCommand]
    private void Logout()
    {
        App.CurrentUser = null;
        WeakReferenceMessenger.Default.Send(new UserStatusChangedMessage(null));
    }
}
