using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RemindMe.Converters;

/// <summary>Null/empty/whitespace string collapses the element; any other text shows it.</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
