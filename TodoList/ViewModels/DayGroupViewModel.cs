using System;
using System.Collections.ObjectModel;
using TodoList.Models;

namespace TodoList.ViewModels;

/// <summary>
/// ViewModel đại diện cho một ngày cụ thể trên màn hình Agenda.
/// </summary>
public class DayGroupViewModel
{
    public DateTime Date { get; set; }

    /// <summary>
    /// Hiển thị định dạng như: "28 Jun • Tomorrow • Sunday" hoặc "29 Jun • Monday"
    /// </summary>
    public string DisplayDate
    {
        get
        {
            var dateStr = Date.ToString("d MMM");
            var dayOfWeek = Date.ToString("dddd");

            if (Date.Date == DateTime.Today)
            {
                return $"{dateStr} • Today • {dayOfWeek}";
            }
            else if (Date.Date == DateTime.Today.AddDays(1))
            {
                return $"{dateStr} • Tomorrow • {dayOfWeek}";
            }
            else if (Date.Date == DateTime.Today.AddDays(-1))
            {
                return $"{dateStr} • Yesterday • {dayOfWeek}";
            }
            
            return $"{dateStr} • {dayOfWeek}";
        }
    }

    /// <summary>
    /// Cho thanh ngày ngang (VD: "Mon")
    /// </summary>
    public string DayName => Date.ToString("ddd");

    /// <summary>
    /// Số ngày (VD: "29")
    /// </summary>
    public string DayNumber => Date.Day.ToString();

    public bool IsToday => Date.Date == DateTime.Today;

    public ObservableCollection<TaskItem> Tasks { get; set; } = new();
}
