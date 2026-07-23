using System.Linq;
using System.Windows;
using RemindMe.Services;

namespace RemindMe.Views;

public partial class HistoryWindow : Window
{
    private readonly StorageService _storage;

    public HistoryWindow(StorageService storage)
    {
        InitializeComponent();
        _storage = storage;
        LoadHistory();
    }

    private void LoadHistory()
    {
        var history = _storage.LoadHistory().OrderByDescending(h => h.EventDate).ToList();
        HistoryList.ItemsSource = history;
        EmptyState.Visibility = history.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        CountText.Text = history.Count == 0 ? "" : $"{history.Count} event{(history.Count == 1 ? "" : "s")}";
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(this, "Clear all history? This cannot be undone.",
            "Clear History", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        _storage.ClearHistory();
        LoadHistory();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
