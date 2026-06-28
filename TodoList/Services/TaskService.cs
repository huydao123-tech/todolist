using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoList.Models;
using TodoList.Providers;

namespace TodoList.Services;

public class TaskService : ITaskService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IGoogleCalendarSyncService _syncService;

    public TaskService(IDbContextFactory<AppDbContext> contextFactory, IGoogleCalendarSyncService syncService)
    {
        _contextFactory = contextFactory;
        _syncService = syncService;
    }

    /// <summary>
    /// Lấy toàn bộ danh sách các tác vụ của một người dùng, kèm theo thông tin List và Tags.
    /// </summary>
    public async Task<List<TaskItem>> GetAllTasksAsync(Guid userId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Tasks
            .Where(t => t.UserId == userId)
            .Include(t => t.List)
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách các tác vụ có hạn chót (DueDate) nằm trong một khoảng thời gian cụ thể của một người dùng.
    /// </summary>
    public async Task<List<TaskItem>> GetTasksByDateRangeAsync(Guid userId, DateTime start, DateTime end)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.Tasks
            .Where(t => t.UserId == userId && t.DueDate >= start && t.DueDate <= end)
            .OrderBy(t => t.DueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Lưu tác vụ mới vào cơ sở dữ liệu SQL Server.
    /// </summary>
    public async Task<TaskItem> AddTaskAsync(TaskItem task)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Tasks.Add(task);
        await context.SaveChangesAsync();
        
        _ = Task.Run(() => _syncService.PushTaskAsync(task).ContinueWith(async t => 
        {
            if (t.IsCompletedSuccessfully) 
            {
                using var innerContext = await _contextFactory.CreateDbContextAsync();
                innerContext.Tasks.Update(task);
                await innerContext.SaveChangesAsync();
            }
        }));

        return task;
    }

    /// <summary>
    /// Lưu các thay đổi của tác vụ hiện tại xuống cơ sở dữ liệu.
    /// </summary>
    public async Task UpdateTaskAsync(TaskItem task)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        context.Tasks.Update(task);
        await context.SaveChangesAsync();

        _ = Task.Run(() => _syncService.PushTaskAsync(task).ContinueWith(async t => 
        {
            if (t.IsCompletedSuccessfully) 
            {
                using var innerContext = await _contextFactory.CreateDbContextAsync();
                innerContext.Tasks.Update(task);
                await innerContext.SaveChangesAsync();
            }
        }));
    }

    /// <summary>
    /// Tìm và xóa tác vụ theo ID chỉ định.
    /// </summary>
    public async Task DeleteTaskAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var task = await context.Tasks.FindAsync(id);
        if (task != null)
        {
            string? googleEventId = task.GoogleEventId;
            context.Tasks.Remove(task);
            await context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(googleEventId))
            {
                _ = Task.Run(() => _syncService.DeleteTaskAsync(googleEventId));
            }
        }
    }
}
