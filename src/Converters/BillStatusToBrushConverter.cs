using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RemindMe.Models;

namespace RemindMe.Converters;

public class BillStatusToBrushConverter : IValueConverter
{
    private static readonly SolidColorBrush Paid = new(Color.FromRgb(0x7B, 0xC9, 0x8F));
    private static readonly SolidColorBrush Overdue = new(Color.FromRgb(0xE0, 0x6C, 0x75));
    private static readonly SolidColorBrush DueSoon = new(Color.FromRgb(0xE5, 0xA8, 0x4C));
    private static readonly SolidColorBrush Upcoming = new(Color.FromRgb(0x8A, 0x9B, 0xC7));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Bill bill) return Upcoming;
        var today = DateTime.Today;

        if (bill.IsPaid) return Paid;
        if (bill.IsOverdue(today)) return Overdue;
        if ((bill.DueDate.Date - today).Days <= 3) return DueSoon;
        return Upcoming;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
