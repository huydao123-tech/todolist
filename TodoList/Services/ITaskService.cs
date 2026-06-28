using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TodoList.Models;

namespace TodoList.Services;

public interface ITaskService
{
    /// <summary>
    /// Lấy toàn bộ danh sách các tác vụ (Task) của một người dùng từ cơ sở dữ liệu.
    /// Bao gồm cả danh mục (List) và thẻ (Tags) đi kèm.
    /// </summary>
    /// <param name="userId">ID người dùng</param>
    /// <returns>Danh sách các tác vụ</returns>
    Task<List<TaskItem>> GetAllTasksAsync(Guid userId);

    /// <summary>
    /// Lấy danh sách các tác vụ có hạn chót (DueDate) nằm trong một khoảng thời gian cụ thể của một người dùng.
    /// Được sử dụng cho màn hình Lịch (Calendar View).
    /// </summary>
    /// <param name="userId">ID người dùng</param>
    /// <param name="start">Ngày bắt đầu</param>
    /// <param name="end">Ngày kết thúc</param>
    /// <returns>Danh sách các tác vụ trong khoảng thời gian được chỉ định</returns>
    Task<List<TaskItem>> GetTasksByDateRangeAsync(Guid userId, DateTime start, DateTime end);

    /// <summary>
    /// Thêm một tác vụ mới vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="task">Đối tượng TaskItem cần thêm</param>
    /// <returns>Tác vụ vừa được thêm thành công</returns>
    Task<TaskItem> AddTaskAsync(TaskItem task);

    /// <summary>
    /// Cập nhật thông tin của một tác vụ đã tồn tại.
    /// </summary>
    /// <param name="task">Đối tượng TaskItem với thông tin mới</param>
    Task UpdateTaskAsync(TaskItem task);

    /// <summary>
    /// Xóa một tác vụ khỏi cơ sở dữ liệu dựa trên ID.
    /// </summary>
    /// <param name="id">Mã định danh (ID) của tác vụ cần xóa</param>
    Task DeleteTaskAsync(Guid id);
}
