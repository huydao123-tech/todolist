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

/// <summary>
/// Converter kiểm tra xem CurrentViewModel có phải là loại được chỉ định (tên ViewModel) hay không.
/// Trả về Visible nếu đúng, Collapsed nếu sai.
/// </summary>
public class ViewModelToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return System.Windows.Visibility.Collapsed;
        var vmType = value.GetType().Name;
        var targetTypeStr = parameter.ToString();
        return vmType == targetTypeStr ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converter so sánh một giá trị int với ConverterParameter.
/// Trả về Visible nếu bằng nhau, Collapsed nếu sai.
/// </summary>
public class IntEqualVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intVal && parameter is string paramStr && int.TryParse(paramStr, out int paramInt))
            return intVal == paramInt ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
