using System;
using System.Linq;
using System.Windows;

namespace RemindMe;

public static class ThemeManager
{
    public static void Apply(bool darkMode)
    {
        var dicts = System.Windows.Application.Current.Resources.MergedDictionaries;
        var existing = dicts.FirstOrDefault(d =>
            d.Source != null && (d.Source.OriginalString.Contains("LightTheme") || d.Source.OriginalString.Contains("DarkTheme")));

        var newSource = new Uri(darkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative);

        if (existing != null)
        {
            var index = dicts.IndexOf(existing);
            dicts[index] = new ResourceDictionary { Source = newSource };
        }
        else
        {
            dicts.Insert(0, new ResourceDictionary { Source = newSource });
        }
    }
}
