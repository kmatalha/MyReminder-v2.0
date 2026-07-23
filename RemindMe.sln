using System;
using System.Globalization;
using System.Windows.Data;
using RemindMe.Models;

namespace RemindMe.Converters;

public class CategoryToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not BillCategory category) return "\uE719";

        // Segoe MDL2 Assets glyphs — modern, built into Windows, no image assets needed.
        return category switch
        {
            BillCategory.Utility => "\uE945",       // lightbulb-ish / info
            BillCategory.Subscription => "\uE8A4",  // repeat / sync
            BillCategory.Fee => "\uE8C7",            // tag
            BillCategory.Rent => "\uE80F",           // home
            BillCategory.Insurance => "\uE72E",      // shield
            BillCategory.Loan => "\uE8C4",           // bank/money
            _ => "\uE719"                             // generic bill/doc
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
