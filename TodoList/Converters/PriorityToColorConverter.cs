using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TodoList.Converters;

/// <summary>
/// Converter chuyển đổi mức độ ưu tiên (High/Medium/Low) thành màu nền tương ứng.
/// </summary>
public class PriorityToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "High"   => new SolidColorBrush(Color.FromRgb(0xFE, 0xE2, 0xE2)), // đỏ nhạt
            "Medium" => new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xCD)), // vàng nhạt
            "Low"    => new SolidColorBrush(Color.FromRgb(0xD1, 0xFA, 0xE5)), // xanh lá nhạt
            _        => new SolidColorBrush(Color.FromRgb(0xFF, 0xF3, 0xCD)),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converter chuyển đổi mức độ ưu tiên thành màu chữ tương ứng.
/// </summary>
public class PriorityToForegroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "High"   => new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)), // đỏ đậm
            "Medium" => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)), // cam/vàng đậm
            "Low"    => new SolidColorBrush(Color.FromRgb(0x05, 0x96, 0x69)), // xanh lá đậm
            _        => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)),
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
