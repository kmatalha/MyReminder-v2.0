using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using RemindMe.Models;
using RemindMe.Services;
using RemindMe.Views;

namespace RemindMe;

public partial class MainWindow : Window
{
    public ObservableCollection<Bill> Bills { get; } = new();

    private readonly StorageService _storage;
    private readonly NotificationService _notifications;
    private readonly ReminderScheduler _scheduler;
    private AppSettings _settings;
    private bool _isLoadingSettingsUi;

    public MainWindow()
    {
        InitializeComponent();

        _storage = App.Storage;
        _notifications = App.Notifications;
        _settings = _storage.LoadSettings();

        BillsList.ItemsSource = Bills;

        _scheduler = new ReminderScheduler(
            _notifications,
            _storage,
            () => Bills,
            () => _settings);

        LoadBills();
        LoadSettingsIntoUi();
        RefreshSummary();

        _scheduler.Start();
    }

    private void LoadBills()
    {
        Bills.Clear();
        foreach (var bill in _storage.LoadBills().OrderBy(b => b.IsPaid).ThenBy(b => b.DueDate))
        {
            Bills.Add(bill);
        }
        SubHeaderText.Text = Bills.Count == 0
            ? "No bills yet — add your first one to get started."
            : $"{Bills.Count(b => !b.IsPaid)} unpaid · {Bills.Count(b => b.IsPaid)} paid";
    }

    private void SaveBills() => _storage.SaveBills(Bills);

    private void LoadSettingsIntoUi()
    {
        _isLoadingSettingsUi = true;
        NotificationStyleCombo.SelectedIndex = _settings.DefaultNotificationStyle switch
        {
            NotificationStyle.Popup => 0,
            NotificationStyle.Banner => 1,
            NotificationStyle.Subtle => 2,
            _ => 1
        };
        StartupCheckBox.IsChecked = _settings.RunAtStartup;
        AlarmSoundNameText.Text = string.IsNullOrWhiteSpace(_settings.CustomAlarmSoundName)
            ? "Default"
            : _settings.CustomAlarmSoundName;
        _isLoadingSettingsUi = false;
    }

    private void RefreshSummary()
    {
        var thisMonth = DateTime.Today;
        var monthBills = Bills.Where(b => b.DueDate.Month == thisMonth.Month && b.DueDate.Year == thisMonth.Year).ToList();
        var paid = monthBills.Count(b => b.IsPaid);
        var unpaid = monthBills.Count - paid;

        SummaryText.Text = monthBills.Count == 0
            ? "No bills due this month."
            : $"{paid} paid · {unpaid} unpaid this month";

        const double maxBarHeight = 60;
        var maxCount = Math.Max(1, Math.Max(paid, unpaid));
        PaidBar.Height = paid == 0 ? 4 : Math.Max(6, maxBarHeight * paid / maxCount);
        UnpaidBar.Height = unpaid == 0 ? 4 : Math.Max(6, maxBarHeight * unpaid / maxCount);
    }

    // ----- Toolbar / sidebar actions -----

    private void AddBillButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddEditBillWindow { Owner = this };
        if (dialog.ShowDialog() == true && dialog.ResultBill is Bill newBill)
        {
            Bills.Add(newBill);
            SaveBills();
            _storage.AppendHistory(MakeHistoryEntry(newBill, HistoryEventType.Added));
            LoadBills();
            RefreshSummary();
        }
    }

    private void HistoryButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new HistoryWindow(_storage) { Owner = this };
        dialog.ShowDialog();
    }

    private void LightTheme_Click(object sender, RoutedEventArgs e) => SetDarkMode(false);
    private void DarkTheme_Click(object sender, RoutedEventArgs e) => SetDarkMode(true);

    private void SetDarkMode(bool dark)
    {
        _settings.DarkMode = dark;
        ThemeManager.Apply(dark);
        _storage.SaveSettings(_settings);
    }

    private void NotificationStyleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isLoadingSettingsUi) return;
        _settings.DefaultNotificationStyle = NotificationStyleCombo.SelectedIndex switch
        {
            0 => NotificationStyle.Popup,
            2 => NotificationStyle.Subtle,
            _ => NotificationStyle.Banner
        };
        _storage.SaveSettings(_settings);
    }

    private void StartupCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_isLoadingSettingsUi) return;
        var enabled = StartupCheckBox.IsChecked == true;
        _settings.RunAtStartup = enabled;
        StartupService.SetStartup(enabled);
        _storage.SaveSettings(_settings);
    }

    private void ChooseAlarmSound_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Choose Alarm Sound",
            Filter = "Audio files (*.wav;*.mp3)|*.wav;*.mp3|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var storedPath = _storage.ImportAlarmSound(dialog.FileName);
            _settings.CustomAlarmSoundPath = storedPath;
            _settings.CustomAlarmSoundName = Path.GetFileName(dialog.FileName);
            _storage.SaveSettings(_settings);
            AlarmSoundNameText.Text = _settings.CustomAlarmSoundName;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Couldn't use that sound file: {ex.Message}", "Alarm Sound",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ResetAlarmSound_Click(object sender, RoutedEventArgs e)
    {
        _settings.CustomAlarmSoundPath = null;
        _settings.CustomAlarmSoundName = null;
        _storage.SaveSettings(_settings);
        AlarmSoundNameText.Text = "Default";
    }

    private void BackupData_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Backup RemindMe Data",
            Filter = "RemindMe backup (*.remindme.json)|*.remindme.json|All files (*.*)|*.*",
            FileName = $"RemindMe-backup-{DateTime.Now:yyyy-MM-dd}.remindme.json"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            _storage.ExportBackup(dialog.FileName, Bills, _storage.LoadHistory(), _settings);
            MessageBox.Show(this, "Backup saved.", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Backup failed: {ex.Message}", "Backup", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RestoreData_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Restore RemindMe Data",
            Filter = "RemindMe backup (*.remindme.json)|*.remindme.json|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != true) return;

        var bundle = _storage.ReadBackup(dialog.FileName);
        if (bundle is null)
        {
            MessageBox.Show(this, "That file isn't a valid RemindMe backup.", "Restore",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var result = MessageBox.Show(this,
            $"Restore from this backup? This replaces your current {Bills.Count} bill(s), history, and settings.",
            "Restore Data", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        _storage.ApplyBackup(bundle);
        _settings = bundle.Settings;
        ThemeManager.Apply(_settings.DarkMode);
        LoadBills();
        LoadSettingsIntoUi();
        RefreshSummary();
        MessageBox.Show(this, "Restore complete.", "Restore", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ----- Bill card actions -----

    private void MarkPaid_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bill bill }) return;
        AnimateCard(sender, () =>
        {
            bill.MarkPaid(DateTime.Now);
            SaveBills();
            _storage.AppendHistory(MakeHistoryEntry(bill, HistoryEventType.Paid));
            LoadBills();
            RefreshSummary();
        });
    }

    private void MarkUnpaid_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bill bill }) return;
        bill.MarkUnpaid();
        SaveBills();
        _storage.AppendHistory(MakeHistoryEntry(bill, HistoryEventType.Unpaid));
        LoadBills();
        RefreshSummary();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bill bill }) return;
        var result = MessageBox.Show(this, $"Delete \"{bill.Name}\"? This cannot be undone.",
            "Delete Bill", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes) return;

        _storage.AppendHistory(MakeHistoryEntry(bill, HistoryEventType.Deleted));
        Bills.Remove(bill);
        SaveBills();
        LoadBills();
        RefreshSummary();
    }

    private void Snooze_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bill bill }) return;
        bill.SnoozeUntil = DateTime.Today.AddDays(1);
        SaveBills();
        LoadBills();
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: Bill bill }) return;
        var dialog = new AddEditBillWindow(bill) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            SaveBills();
            LoadBills();
            RefreshSummary();
        }
    }

    public void HandleToastAction(Guid billId, string? action)
    {
        var bill = Bills.FirstOrDefault(b => b.Id == billId);
        if (bill is null) return;

        switch (action)
        {
            case "pay":
                bill.MarkPaid(DateTime.Now);
                _storage.AppendHistory(MakeHistoryEntry(bill, HistoryEventType.Paid));
                break;
            case "snooze":
                bill.SnoozeUntil = DateTime.Today.AddDays(1);
                break;
        }

        SaveBills();
        LoadBills();
        RefreshSummary();
    }

    private static HistoryEntry MakeHistoryEntry(Bill bill, HistoryEventType type) => new()
    {
        BillId = bill.Id,
        BillName = bill.Name,
        Category = bill.Category,
        Description = bill.Description,
        DueDate = bill.DueDate,
        EventDate = DateTime.Now,
        EventType = type
    };

    /// <summary>Quick, satisfying scale + fade pulse on the card before it re-sorts to "paid".</summary>
    private void AnimateCard(object sender, Action onComplete)
    {
        if (sender is not Button button || FindAncestor<Border>(button) is not Border card)
        {
            onComplete();
            return;
        }

        if (card.RenderTransform is not ScaleTransform)
        {
            card.RenderTransform = new ScaleTransform(1, 1);
        }

        var scale = (ScaleTransform)card.RenderTransform;
        var storyboard = new Storyboard();

        var scaleUp = new DoubleAnimation(1, 1.03, TimeSpan.FromMilliseconds(120))
        {
            AutoReverse = true,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(scaleUp, card);
        Storyboard.SetTargetProperty(scaleUp, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));
        storyboard.Children.Add(scaleUp);

        var scaleUpY = scaleUp.Clone();
        Storyboard.SetTarget(scaleUpY, card);
        Storyboard.SetTargetProperty(scaleUpY, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));
        storyboard.Children.Add(scaleUpY);

        storyboard.Completed += (_, _) => onComplete();
        storyboard.Begin();
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T match) return match;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    // ----- Window lifecycle: minimize to tray instead of closing -----

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}
