using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Linq;
using System.Threading.Tasks;
using TodoList.Messages;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class MainViewModel : ObservableObject, 
    IRecipient<NavigationMessage>, 
    IRecipient<UserStatusChangedMessage>,
    IRecipient<TaskDataChangedMessage>
{
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CalendarViewModel _calendarViewModel;
    private readonly CreateTaskViewModel _createTaskViewModel;
    private readonly EisenhowerViewModel _eisenhowerViewModel;
    private readonly MindSandboxViewModel _mindSandboxViewModel;
    private readonly TaDaListViewModel _taDaListViewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly IGoogleCalendarSyncService _syncService;

    public DashboardViewModel DashboardViewModel => _dashboardViewModel;
    public CalendarViewModel CalendarViewModel => _calendarViewModel;
    public CreateTaskViewModel CreateTaskViewModel => _createTaskViewModel;
    public EisenhowerViewModel EisenhowerViewModel => _eisenhowerViewModel;
    public MindSandboxViewModel MindSandboxViewModel => _mindSandboxViewModel;
    public TaDaListViewModel TaDaListViewModel => _taDaListViewModel;

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
        MindSandboxViewModel mindSandboxViewModel,
        TaDaListViewModel taDaListViewModel,
        IServiceProvider serviceProvider,
        IGoogleCalendarSyncService syncService)
    {
        _dashboardViewModel = dashboardViewModel;
        _calendarViewModel = calendarViewModel;
        _createTaskViewModel = createTaskViewModel;
        _eisenhowerViewModel = eisenhowerViewModel;
        _mindSandboxViewModel = mindSandboxViewModel;
        _taDaListViewModel = taDaListViewModel;
        _serviceProvider = serviceProvider;
        _syncService = syncService;
        
        WeakReferenceMessenger.Default.Register<NavigationMessage>(this);
        WeakReferenceMessenger.Default.Register<UserStatusChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<TaskDataChangedMessage>(this);

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
        else if (message.Value == "MindSandbox")
        {
            NavigateToMindSandbox();
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

    public void Receive(TaskDataChangedMessage message)
    {
        var source = message.Source;
        var dashboardChanged = ReferenceEquals(source, _dashboardViewModel);
        var calendarChanged = ReferenceEquals(source, _calendarViewModel);
        var eisenhowerChanged = ReferenceEquals(source, _eisenhowerViewModel);

        if (!dashboardChanged)
            _dashboardViewModel.InvalidateCache();
        if (!calendarChanged)
            _calendarViewModel.InvalidateCache();
        if (!eisenhowerChanged)
            _eisenhowerViewModel.InvalidateCache();

        if (CurrentViewModel == _dashboardViewModel && !dashboardChanged)
        {
            _ = _dashboardViewModel.LoadTasksAsync(forceReload: true);
        }
        else if (CurrentViewModel == _calendarViewModel && !calendarChanged)
        {
            _ = _calendarViewModel.LoadWeekAsync(forceReload: true);
        }
        else if (CurrentViewModel == _eisenhowerViewModel && !eisenhowerChanged)
        {
            _ = _eisenhowerViewModel.LoadDataAsync(forceReload: true);
        }
    }

    private async Task AutoSyncAfterLoginAsync()
    {
        try
        {
            var taskService = (ITaskService)_serviceProvider.GetService(typeof(ITaskService))!;
            var allTasks = await taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
            bool hasSyncedBefore = allTasks.Any(t => !string.IsNullOrEmpty(t.GoogleEventId));
            
            if (hasSyncedBefore)
            {
                // Chỉ sync lần đầu tiên, nếu đã có dữ liệu Google Calendar thì bỏ qua
                return;
            }

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
        CurrentViewModel = _dashboardViewModel;
        _dashboardViewModel.SelectedTabIndex = 1;
        _ = _dashboardViewModel.LoadTasksAsync(forceReload: false);
    }

    [RelayCommand]
    private void NavigateToToday()
    {
        ActivePage = "Today";
        CurrentViewModel = _dashboardViewModel;
        _dashboardViewModel.SelectedTabIndex = 0;
        _ = _dashboardViewModel.LoadTasksAsync(forceReload: false);
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Lịch (Calendar).
    /// </summary>
    [RelayCommand]
    private void NavigateToCalendar()
    {
        ActivePage = "Calendar";
        CurrentViewModel = _calendarViewModel;
        _ = _calendarViewModel.LoadWeekAsync(forceReload: false);
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Ma trận Eisenhower.
    /// </summary>
    [RelayCommand]
    private void NavigateToEisenhower()
    {
        ActivePage = "Eisenhower";
        CurrentViewModel = _eisenhowerViewModel;
        _ = _eisenhowerViewModel.LoadDataAsync(forceReload: false);
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Mind Sandbox.
    /// </summary>
    [RelayCommand]
    private void NavigateToMindSandbox()
    {
        ActivePage = "MindSandbox";
        CurrentViewModel = _mindSandboxViewModel;
        _ = _mindSandboxViewModel.LoadIdeasAsync();
    }

    /// <summary>
    /// Chuyển hướng sang màn hình Ta-Da List.
    /// </summary>
    [RelayCommand]
    private void NavigateToTaDaList()
    {
        ActivePage = "TaDaList";
        CurrentViewModel = _taDaListViewModel;
        _ = _taDaListViewModel.LoadDataAsync();
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
