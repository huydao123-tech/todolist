using System;
using System.Globalization;
using System.Windows.Data;

namespace TodoList.Converters;

/// <summary>
/// Converter so sánh một giá trị int với ConverterParameter.
/// Trả về True nếu bằng nhau (dùng cho RadioButton IsChecked binding).
/// </summary>
public class IntEqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intVal && parameter is string paramStr && int.TryParse(paramStr, out int paramInt))
            return intVal == paramInt;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked && parameter is string paramStr && int.TryParse(paramStr, out int paramInt))
            return paramInt;
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converter so sánh một giá trị string với ConverterParameter.
/// Trả về True nếu bằng nhau (dùng cho RadioButton IsChecked binding).
/// </summary>
public class StringEqualConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() == parameter?.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
            return parameter?.ToString() ?? Binding.DoNothing;
        return Binding.DoNothing;
    }
}
