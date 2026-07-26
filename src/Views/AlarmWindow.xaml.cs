using System;
using System.ComponentModel;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace RemindMe.Views;

public partial class AlarmWindow : Window
{
    public enum AlarmResult { None, Paid, Snoozed, Dismissed }

    public AlarmResult Result { get; private set; } = AlarmResult.None;

    private MediaPlayer? _mediaPlayer;
    private DispatcherTimer? _beepTimer;
    private readonly DispatcherTimer _autoStopTimer;

    public AlarmWindow(string title, string subtitle, string? description, string? customSoundPath, bool showBillButtons)
    {
        InitializeComponent();

        TitleText.Text = title;
        SubtitleText.Text = subtitle;
        DescriptionText.Text = description ?? string.Empty;
        DescriptionText.Visibility = string.IsNullOrWhiteSpace(description) ? Visibility.Collapsed : Visibility.Visible;

        MarkPaidButton.Visibility = showBillButtons ? Visibility.Visible : Visibility.Collapsed;
        SnoozeButton.Visibility = showBillButtons ? Visibility.Visible : Visibility.Collapsed;

        StartSound(customSoundPath);

        // Safety net so a forgotten/ignored alarm doesn't ring forever - stop sound after 3 minutes,
        // the window itself stays open until the user dismisses it.
        _autoStopTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(3) };
        _autoStopTimer.Tick += (_, _) => StopSound();
        _autoStopTimer.Start();

        Loaded += (_, _) => ForceToForeground();
    }

    /// <summary>
    /// This window is the "guaranteed" alarm, so it can't rely on being owned by MainWindow -
    /// MainWindow is routinely Hidden (minimized to tray), and a Window whose Owner is not
    /// currently visible can fail to actually appear on screen or come to the foreground on
    /// Windows. This window is deliberately shown with no Owner (see MainWindow.ShowAlarmForBill
    /// and TestAlarm_Click) and forces itself to the foreground here as a second safety net,
    /// since Windows can otherwise refuse focus to a background process's new window.
    /// </summary>
    private void ForceToForeground()
    {
        try
        {
            Show();
            WindowState = WindowState.Normal;
            Topmost = false;
            Topmost = true;
            Activate();

            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
            {
                SetForegroundWindow(hwnd);
            }
        }
        catch
        {
            // Best effort - the window is already Show()n and Topmost via XAML either way.
        }
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private void StartSound(string? customSoundPath)
    {
        if (!string.IsNullOrWhiteSpace(customSoundPath) && File.Exists(customSoundPath))
        {
            try
            {
                _mediaPlayer = new MediaPlayer { Volume = 1.0 };
                _mediaPlayer.MediaEnded += (_, _) =>
                {
                    _mediaPlayer.Position = TimeSpan.Zero;
                    _mediaPlayer.Play();
                };
                _mediaPlayer.Open(new Uri(customSoundPath));
                _mediaPlayer.Play();
                return;
            }
            catch
            {
                // Fall through to the guaranteed system-sound fallback below.
                _mediaPlayer = null;
            }
        }

        // No custom sound (or it failed to load) - fall back to a repeating system alert sound.
        // This has no external dependencies and will always play as long as system sound isn't muted.
        SystemSounds.Exclamation.Play();
        _beepTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.3) };
        _beepTimer.Tick += (_, _) => SystemSounds.Exclamation.Play();
        _beepTimer.Start();
    }

    private void StopSound()
    {
        _beepTimer?.Stop();
        _beepTimer = null;
        _autoStopTimer.Stop();

        if (_mediaPlayer is not null)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.Close();
            _mediaPlayer = null;
        }
    }

    private void MarkPaid_Click(object sender, RoutedEventArgs e)
    {
        Result = AlarmResult.Paid;
        Close();
    }

    private void Snooze_Click(object sender, RoutedEventArgs e)
    {
        Result = AlarmResult.Snoozed;
        Close();
    }

    private void Dismiss_Click(object sender, RoutedEventArgs e)
    {
        Result = AlarmResult.Dismissed;
        Close();
    }

    private void AlarmWindow_Closing(object? sender, CancelEventArgs e) => StopSound();
}
