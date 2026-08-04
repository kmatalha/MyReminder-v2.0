using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
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

        // Catch anything that would otherwise crash the whole app silently (this is the
        // single biggest reason an alarm can appear to "do nothing" - an unhandled
        // exception on the UI thread kills the process with no dialog and no log).
        DispatcherUnhandledException += (_, args) =>
        {
            Logger.LogError("DispatcherUnhandledException", args.Exception);
            MessageBox.Show(
                $"RemindMe hit an unexpected error and would have closed:\n\n{args.Exception.Message}\n\nDetails were written to debug.log.",
                "RemindMe - Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true; // keep the app alive instead of crashing
        };

        Logger.Log("App starting up.");

        try
        {
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

            Logger.Log("App startup completed successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError("OnStartup", ex);
            MessageBox.Show(
                $"RemindMe failed to start:\n\n{ex.Message}\n\nDetails were written to debug.log.",
                "RemindMe - Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // GetResourceStream needs a *relative* pack URI, not "pack://application:,,,/...".
            var streamInfo = GetResourceStream(new Uri("Assets/AppIcon.ico", UriKind.Relative));
            if (streamInfo is not null)
            {
                using var stream = streamInfo.Stream;
                return new Drawing.Icon(stream);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("LoadTrayIcon", ex);
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
