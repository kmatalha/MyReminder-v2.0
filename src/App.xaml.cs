using System;
using System.Linq;
using System.Windows;
using RemindMe.Services;
using RemindMe.Views;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;

namespace RemindMe;

public partial class App : System.Windows.Application
{
    public static StorageService Storage { get; private set; } = null!;
    public static NotificationService Notifications { get; private set; } = null!;

    private Forms.NotifyIcon? _trayIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Previously there were no global handlers at all: any uncaught exception on the UI
        // thread (including inside the reminder scheduler's timer tick, before this fix isolated
        // it) would silently terminate the entire process - no crash dialog, no window, nothing.
        // That's indistinguishable from "the alarm just didn't ring". These handlers make sure
        // that can't happen again: log it, and keep the app running whenever possible.
        DispatcherUnhandledException += (_, args) =>
        {
            Logger.LogException("DispatcherUnhandledException", args.Exception);
            args.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex) Logger.LogException("AppDomain.UnhandledException", ex);
        };
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            Logger.LogException("UnobservedTaskException", args.Exception);
            args.SetObserved();
        };

        Storage = new StorageService();
        Notifications = new NotificationService();

        ShortcutHelper.EnsureAppIdentity();

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
            Icon = LoadTrayIcon(),
            Visible = true,
            Text = "RemindMe"
        };

        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open RemindMe", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Quit", null, (_, _) => QuitApp());
        _trayIcon.ContextMenuStrip = menu;
        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    /// <summary>Loads the app's own icon (embedded as a WPF resource, so this works in both
    /// loose-file and single-file-published builds) - falls back to the generic system icon
    /// if anything goes wrong.</summary>
    private static Drawing.Icon LoadTrayIcon()
    {
        try
        {
            var streamInfo = GetResourceStream(new Uri("pack://application:,,,/Assets/AppIcon.ico"));
            if (streamInfo is not null)
            {
                using var stream = streamInfo.Stream;
                return new Drawing.Icon(stream);
            }
        }
        catch
        {
            // Fall through to the system default below.
        }
        return Drawing.SystemIcons.Application;
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
