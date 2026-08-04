using System;
using System.IO;

namespace RemindMe.Services;

/// <summary>
/// Minimal append-only log file, so alarm/notification problems leave a trace instead of
/// failing completely silently. Written to %AppData%\RemindMe\debug.log.
/// </summary>
public static class Logger
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RemindMe", "debug.log");

    private static readonly object Lock = new();

    public static void Log(string message)
    {
        try
        {
            lock (Lock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                File.AppendAllText(LogPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
            // Logging must never itself crash the app.
        }
    }

    public static void LogError(string context, Exception ex) =>
        Log($"ERROR in {context}: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
}
