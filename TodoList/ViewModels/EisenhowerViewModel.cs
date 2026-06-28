using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TodoList.Models;
using TodoList.Services;

namespace TodoList.ViewModels;

public partial class EisenhowerViewModel : ObservableObject
{
    private readonly ITaskService _taskService;

    // Giới hạn WIP cho ô P1
    private const int P1_WIP_LIMIT = 5;

    // ===== Các danh sách công việc theo vùng =====
    [ObservableProperty] private ObservableCollection<TaskItem> _inboxTasks    = new();
    [ObservableProperty] private ObservableCollection<TaskItem> _p1Tasks       = new();
    [ObservableProperty] private ObservableCollection<TaskItem> _p2Tasks       = new();
    [ObservableProperty] private ObservableCollection<TaskItem> _p3Tasks       = new();
    [ObservableProperty] private ObservableCollection<TaskItem> _p4Tasks       = new();

    // ===== Brain Dump =====
    [ObservableProperty] private string _newInboxTitle = string.Empty;

    // ===== WIP Limit Warning =====
    [ObservableProperty] private bool   _isP1Overloaded;
    [ObservableProperty] private string _p1CountLabel = "0/5";

    // ===== P2 Schedule Suggestion =====
    [ObservableProperty] private bool        _hasP2Suggestion;
    [ObservableProperty] private string      _p2SuggestionText = string.Empty;
    [ObservableProperty] private TaskItem?   _p2SuggestedTask;

    // ===== Analytics =====
    [ObservableProperty] private int   _totalDone;
    [ObservableProperty] private int   _p1Done;
    [ObservableProperty] private int   _p2Done;
    [ObservableProperty] private int   _p3Done;
    [ObservableProperty] private int   _p4Done;
    [ObservableProperty] private int   _totalActive;
    [ObservableProperty] private int   _p1Count;
    [ObservableProperty] private int   _p2Count;
    [ObservableProperty] private int   _p3Count;
    [ObservableProperty] private int   _p4Count;
    [ObservableProperty] private string _analyticsAdvice = string.Empty;
    [ObservableProperty] private string _analyticsAdviceIcon = "💡";

    // Tab switcher: 0 = Matrix, 1 = Analytics
    [ObservableProperty] private int _selectedTabIndex;

    public EisenhowerViewModel(ITaskService taskService)
    {
        _taskService = taskService;
        _ = LoadDataAsync();
    }

    // ───────────────────────────────────────────────────────────
    //  DATA LOADING
    // ───────────────────────────────────────────────────────────

    public async Task LoadDataAsync()
    {
        try
        {
            var all = await _taskService.GetAllTasksAsync(App.CurrentUser?.Id ?? App.DefaultUserId);
            var active = all.Where(t => !t.DeletedAt.HasValue).ToList();

            InboxTasks = new ObservableCollection<TaskItem>(
                active.Where(t => t.Quadrant == EisenhowerQuadrant.Inbox));
            P1Tasks = new ObservableCollection<TaskItem>(
                active.Where(t => t.Quadrant == EisenhowerQuadrant.P1_Do));
            P2Tasks = new ObservableCollection<TaskItem>(
                active.Where(t => t.Quadrant == EisenhowerQuadrant.P2_Schedule));
            P3Tasks = new ObservableCollection<TaskItem>(
                active.Where(t => t.Quadrant == EisenhowerQuadrant.P3_Delegate));
            P4Tasks = new ObservableCollection<TaskItem>(
                active.Where(t => t.Quadrant == EisenhowerQuadrant.P4_Eliminate));

            UpdateP1WipStatus();
            UpdateP2Suggestion();
            UpdateAnalytics(active);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ───────────────────────────────────────────────────────────
    //  WIP LIMIT P1
    // ───────────────────────────────────────────────────────────

    private void UpdateP1WipStatus()
    {
        P1Count      = P1Tasks.Count;
        P1CountLabel = $"{P1Count}/{P1_WIP_LIMIT}";
        IsP1Overloaded = P1Count >= P1_WIP_LIMIT;
    }

    // ───────────────────────────────────────────────────────────
    //  P2 SCHEDULE SUGGESTION
    // ───────────────────────────────────────────────────────────

    private void UpdateP2Suggestion()
    {
        // Tìm task P2 chưa có DueDate hoặc đã tồn tại quá 3 ngày không có lịch
        var unscheduled = P2Tasks
            .Where(t => t.DueDate == null || t.DueDate < DateTime.Today)
            .OrderBy(t => t.UpdatedAt)
            .FirstOrDefault();

        if (unscheduled != null)
        {
            P2SuggestedTask  = unscheduled;
            P2SuggestionText = $"📅 Phát hiện công việc quan trọng chưa có lịch: \"{unscheduled.Title}\". Bạn có muốn đặt lịch cho ngày mai không?";
            HasP2Suggestion  = true;
        }
        else
        {
            HasP2Suggestion = false;
            P2SuggestedTask = null;
        }
    }

    // ───────────────────────────────────────────────────────────
    //  ANALYTICS
    // ───────────────────────────────────────────────────────────

    private void UpdateAnalytics(System.Collections.Generic.List<TaskItem> active)
    {
        P1Count    = active.Count(t => t.Quadrant == EisenhowerQuadrant.P1_Do);
        P2Count    = active.Count(t => t.Quadrant == EisenhowerQuadrant.P2_Schedule);
        P3Count    = active.Count(t => t.Quadrant == EisenhowerQuadrant.P3_Delegate);
        P4Count    = active.Count(t => t.Quadrant == EisenhowerQuadrant.P4_Eliminate);

        P1Done     = active.Count(t => t.Quadrant == EisenhowerQuadrant.P1_Do       && t.Status == "DONE");
        P2Done     = active.Count(t => t.Quadrant == EisenhowerQuadrant.P2_Schedule && t.Status == "DONE");
        P3Done     = active.Count(t => t.Quadrant == EisenhowerQuadrant.P3_Delegate && t.Status == "DONE");
        P4Done     = active.Count(t => t.Quadrant == EisenhowerQuadrant.P4_Eliminate && t.Status == "DONE");
        TotalDone  = P1Done + P2Done + P3Done + P4Done;
        TotalActive = active.Count;

        int total = P1Count + P2Count + P3Count + P4Count;
        if (total == 0) { AnalyticsAdvice = "Hãy bắt đầu thêm công việc và phân loại chúng vào ma trận!"; AnalyticsAdviceIcon = "🚀"; return; }

        double p1Ratio = (double)P1Count / total;
        double p2Ratio = (double)P2Count / total;
        double p3Ratio = (double)P3Count / total;
        double p4Ratio = (double)P4Count / total;

        if (p2Ratio >= 0.5)
        {
            AnalyticsAdvice    = "Tuyệt vời! Bạn đang dành phần lớn thời gian cho ô P2 (Phát triển bản thân). Đây là dấu hiệu của người làm chủ cuộc sống và chủ động kiểm soát tương lai!";
            AnalyticsAdviceIcon = "🏆";
        }
        else if (p1Ratio >= 0.5)
        {
            AnalyticsAdvice    = "⚠️ Cảnh báo: Bạn đang có quá nhiều việc khủng hoảng ở ô P1. Hãy dành nhiều thời gian hơn cho ô P2 (lên kế hoạch, học kỹ năng, phòng ngừa sớm) để ô P1 không còn quá tải nữa.";
            AnalyticsAdviceIcon = "⚠️";
        }
        else if (p3Ratio >= 0.4)
        {
            AnalyticsAdvice    = "Bạn đang bị nhiễu loạn bởi quá nhiều việc thuộc ô P3 (Nhiễu). Hãy học cách nói 'Không', ủy quyền mạnh dạn hơn, và dùng công cụ tự động hóa để giải phóng thời gian cho P2.";
            AnalyticsAdviceIcon = "🔇";
        }
        else if (p4Ratio >= 0.3)
        {
            AnalyticsAdvice    = "Bạn đang lãng phí khá nhiều thời gian cho việc ô P4 (Lãng phí). Khi bạn có nhiều việc quan trọng cần làm, hãy mạnh dạn loại bỏ các hoạt động này.";
            AnalyticsAdviceIcon = "🗑️";
        }
        else
        {
            AnalyticsAdvice    = "Bạn đang có một ma trận cân bằng tốt! Tiếp tục duy trì và tăng dần tỷ lệ P2 để đạt đến trạng thái \"chủ động\" hoàn toàn.";
            AnalyticsAdviceIcon = "✅";
        }
    }

    // ───────────────────────────────────────────────────────────
    //  COMMANDS: BRAIN DUMP
    // ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task AddToInboxAsync()
    {
        if (string.IsNullOrWhiteSpace(NewInboxTitle)) return;
        var task = new TaskItem
        {
            UserId   = App.CurrentUser?.Id ?? App.DefaultUserId,
            Title    = NewInboxTitle,
            Quadrant = EisenhowerQuadrant.Inbox,
            Status   = "TODO"
        };
        try
        {
            var added = await _taskService.AddTaskAsync(task);
            InboxTasks.Add(added);
            NewInboxTitle = string.Empty;
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể thêm công việc:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ───────────────────────────────────────────────────────────
    //  COMMANDS: MOVE TO QUADRANT (4 lệnh riêng biệt)
    // ───────────────────────────────────────────────────────────

    [RelayCommand] private Task MoveToP1Async(TaskItem task) => MoveToQuadrantCoreAsync(task, EisenhowerQuadrant.P1_Do);
    [RelayCommand] private Task MoveToP2Async(TaskItem task) => MoveToQuadrantCoreAsync(task, EisenhowerQuadrant.P2_Schedule);
    [RelayCommand] private Task MoveToP3Async(TaskItem task) => MoveToQuadrantCoreAsync(task, EisenhowerQuadrant.P3_Delegate);
    [RelayCommand] private Task MoveToP4Async(TaskItem task) => MoveToQuadrantCoreAsync(task, EisenhowerQuadrant.P4_Eliminate);

    private async Task MoveToQuadrantCoreAsync(TaskItem task, EisenhowerQuadrant targetQuadrant)
    {
        if (task == null) return;

        // Kiểm tra WIP Limit khi chuyển vào P1
        if (targetQuadrant == EisenhowerQuadrant.P1_Do && P1Tasks.Count >= P1_WIP_LIMIT)
        {
            var result = MessageBox.Show(
                $"Ô Khẩn cấp (P1) đang có {P1Tasks.Count} việc — đã đạt giới hạn tối đa {P1_WIP_LIMIT} việc!\n\n" +
                "Quá nhiều việc ở ô P1 sẽ khiến bạn bị stress kéo dài.\n\n" +
                "Bạn có muốn chuyển công việc này sang ô Lên lịch (P2) để tránh tình trạng này không?",
                "⚠️ Cảnh báo: Ô P1 Quá tải!",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
                targetQuadrant = EisenhowerQuadrant.P2_Schedule;
            // Nếu No → vẫn cho lưu vào P1
        }

        task.Quadrant  = targetQuadrant;
        task.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _taskService.UpdateTaskAsync(task);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể di chuyển công việc:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ───────────────────────────────────────────────────────────
    //  COMMANDS: DELETE TASK
    // ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task DeleteTaskAsync(TaskItem task)
    {
        if (task == null) return;
        var result = MessageBox.Show($"Xóa công việc '{task.Title}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;
        try
        {
            await _taskService.DeleteTaskAsync(task.Id);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể xóa:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ───────────────────────────────────────────────────────────
    //  COMMANDS: COMPLETE TASK
    // ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task ToggleDoneAsync(TaskItem task)
    {
        if (task == null) return;
        task.Status    = task.Status == "DONE" ? "TODO" : "DONE";
        task.UpdatedAt = DateTime.UtcNow;
        try
        {
            await _taskService.UpdateTaskAsync(task);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể cập nhật:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ───────────────────────────────────────────────────────────
    //  COMMANDS: P2 SCHEDULE SUGGESTION
    // ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task AcceptP2SuggestionAsync()
    {
        if (P2SuggestedTask == null) return;
        P2SuggestedTask.DueDate    = DateTime.Today.AddDays(1);
        P2SuggestedTask.UpdatedAt  = DateTime.UtcNow;
        try
        {
            await _taskService.UpdateTaskAsync(P2SuggestedTask);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể cập nhật lịch:\n{ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DismissP2Suggestion() => HasP2Suggestion = false;
}
