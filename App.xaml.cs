using System;
using System.Linq;
using System.Windows;
using RemindMe.Services;
using RemindMe.Views;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace RemindMe;

public partial class App : Application
{
    public static StorageService Storage { get; private set; } = null!;
    public static NotificationService Notifications { get; private set; } = null!;

    private Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Storage = new StorageService();
        Notifications = new NotificationService();

        ShortcutHelper.EnsureShortcutWithAumid();

        var settings = Storage.LoadSettings();
        ThemeManager.Apply(settings.DarkMode);

        SetupTrayIcon();

        _mainWindow = new MainWindow();

        Notifications.ToastActionInvoked += (billId, action) =>
        {
            Dispatcher.Invoke(() =>
            {
                _mainWindow.HandleToastAction(billId, action);
                if (action is null)
                {
                    // Plain body tap: bring the app to the foreground so the user can look around.
                    ShowMainWindow();
                }
            });
        };

        var startMinimized = e.Args.Contains("--minimized");
        if (!startMinimized)
        {
            ShowMainWindow();
        }
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Application,
            Visible = true,
            Text = "RemindMe"
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open RemindMe", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Quit", null, (_, _) => QuitApp());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    public void ShowMainWindow()
    {
        if (_mainWindow is null) return;
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    public void QuitApp()
    {
        _trayIcon?.Dispose();
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        base.OnExit(e);
    }
}
