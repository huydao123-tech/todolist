using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoList.Models;
using TodoList.Providers;

namespace TodoList.Services;

public interface IGoogleCalendarSyncService
{
    Task PushTaskAsync(TaskItem taskItem);
    Task DeleteTaskAsync(string googleEventId);
    Task PullTasksAsync(IServiceProvider serviceProvider);
}

public class GoogleCalendarSyncService : IGoogleCalendarSyncService
{
    private static readonly string[] Scopes = { CalendarService.Scope.CalendarEvents };
    private static readonly string ApplicationName = "TodoList App";
    private CalendarService? _service;

    public GoogleCalendarSyncService()
    {
    }

    private async Task<CalendarService> GetServiceAsync()
    {
        if (_service != null) return _service;

        UserCredential credential;
        string credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\google calendar\credentials.json");
        // Đảm bảo đường dẫn đúng khi chạy từ Debug (bin/Debug/net8.0-windows)
        if (!File.Exists(credPath))
        {
            credPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "google calendar", "credentials.json");
        }
        if (!File.Exists(credPath))
        {
            credPath = "google calendar/credentials.json";
        }

        using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
        {
            // Token lưu ở thư mục gốc của app
            string credStorePath = "token.json";
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore(credStorePath, true));
        }

        _service = new CalendarService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        return _service;
    }

    public async Task PushTaskAsync(TaskItem taskItem)
    {
        if (taskItem.DeletedAt.HasValue) return; // Đã xóa

        var service = await GetServiceAsync();

        Event eventBody = new Event
        {
            Summary = taskItem.Title,
            Description = taskItem.Description
        };

        if (taskItem.DueDate.HasValue)
        {
            var startDate = taskItem.DueDate.Value;
            var endDate = taskItem.EndTime ?? startDate.AddHours(1);

            eventBody.Start = new EventDateTime { DateTimeDateTimeOffset = startDate };
            eventBody.End = new EventDateTime { DateTimeDateTimeOffset = endDate };
        }
        else
        {
            var date = DateTime.Today;
            eventBody.Start = new EventDateTime { Date = date.ToString("yyyy-MM-dd") };
            eventBody.End = new EventDateTime { Date = date.AddDays(1).ToString("yyyy-MM-dd") };
        }

        try
        {
            if (string.IsNullOrEmpty(taskItem.GoogleEventId))
            {
                // Create
                var request = service.Events.Insert(eventBody, "primary");
                var createdEvent = await request.ExecuteAsync();
                taskItem.GoogleEventId = createdEvent.Id;
                taskItem.LastSyncedAt = DateTime.UtcNow;
            }
            else
            {
                // Update
                var request = service.Events.Update(eventBody, "primary", taskItem.GoogleEventId);
                var updatedEvent = await request.ExecuteAsync();
                taskItem.LastSyncedAt = DateTime.UtcNow;
            }
        }
        catch (Exception)
        {
            // Nếu lỗi (ví dụ event bị xóa trên google calendar), thử tạo lại
            if (!string.IsNullOrEmpty(taskItem.GoogleEventId))
            {
                taskItem.GoogleEventId = null;
                await PushTaskAsync(taskItem);
            }
        }
    }

    public async Task DeleteTaskAsync(string googleEventId)
    {
        if (string.IsNullOrEmpty(googleEventId)) return;
        var service = await GetServiceAsync();
        try
        {
            await service.Events.Delete("primary", googleEventId).ExecuteAsync();
        }
        catch (Exception)
        {
            // Bỏ qua nếu đã bị xóa trên Google Calendar
        }
    }

    public async Task PullTasksAsync(IServiceProvider serviceProvider)
    {
        var service = await GetServiceAsync();
        
        // Lấy các sự kiện trong vòng 2 năm (1 năm trước tới 1 năm sau)
        var request = service.Events.List("primary");
        request.TimeMinDateTimeOffset = DateTimeOffset.UtcNow.AddYears(-1);
        request.TimeMaxDateTimeOffset = DateTimeOffset.UtcNow.AddYears(1);
        request.ShowDeleted = true;
        request.SingleEvents = true;
        request.MaxResults = 2500; // Tối đa mỗi trang
        
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // === Tối ưu hiệu năng: Load toàn bộ GoogleEventId của user vào Dictionary một lần ===
        var userId = App.CurrentUser?.Id ?? App.DefaultUserId;
        var existingTasksDict = dbContext.Tasks
            .Where(t => t.UserId == userId && t.GoogleEventId != null)
            .ToDictionary(t => t.GoogleEventId!, t => t);
        
        string? pageToken = null;
        
        do
        {
            request.PageToken = pageToken;
            var events = await request.ExecuteAsync();
            
            if (events.Items == null) break;
            
            foreach (var evt in events.Items)
            {
                // Tra cứu trong dictionary thay vì query DB từng lần (O(1) thay vì O(N))
                existingTasksDict.TryGetValue(evt.Id, out var existingTask);
                
                if (evt.Status == "cancelled")
                {
                    if (existingTask != null && !existingTask.DeletedAt.HasValue)
                    {
                        existingTask.DeletedAt = DateTime.UtcNow;
                        dbContext.Tasks.Update(existingTask);
                    }
                    continue;
                }
                
                DateTime? startDate = evt.Start?.DateTimeDateTimeOffset?.DateTime 
                    ?? (evt.Start?.Date != null ? DateTime.Parse(evt.Start.Date) : null);
                DateTime? endDate = evt.End?.DateTimeDateTimeOffset?.DateTime 
                    ?? (evt.End?.Date != null ? DateTime.Parse(evt.End.Date) : null);
                
                if (existingTask == null)
                {
                    var newTask = new TaskItem
                    {
                        UserId = userId,
                        Title = evt.Summary ?? "Untitled Event",
                        Description = evt.Description,
                        DueDate = startDate,
                        EndTime = endDate,
                        GoogleEventId = evt.Id,
                        LastSyncedAt = DateTime.UtcNow,
                        Status = "TODO"
                    };
                    dbContext.Tasks.Add(newTask);
                    // Thêm vào dict để tránh insert trùng trong trang tiếp
                    existingTasksDict[evt.Id] = newTask;
                }
                else
                {
                    DateTime eventUpdated = evt.UpdatedDateTimeOffset?.UtcDateTime ?? DateTime.UtcNow;
                    if (existingTask.LastSyncedAt == null || eventUpdated > existingTask.LastSyncedAt)
                    {
                        existingTask.Title = evt.Summary ?? "Untitled Event";
                        existingTask.Description = evt.Description;
                        existingTask.DueDate = startDate;
                        existingTask.EndTime = endDate;
                        existingTask.LastSyncedAt = DateTime.UtcNow;
                        dbContext.Tasks.Update(existingTask);
                    }
                }
            }
            
            pageToken = events.NextPageToken;
        } while (pageToken != null);
        
        await dbContext.SaveChangesAsync();
    }
}
