using System;
using System.Globalization;
using System.Windows.Data;

namespace TodoList.Converters;

public class StatusToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status == "DONE";
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked)
        {
            return isChecked ? "DONE" : "TODO";
        }
        return "TODO";
    }
}
