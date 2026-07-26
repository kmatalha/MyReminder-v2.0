using System;
using System.IO;

namespace RemindMe.Services;

/// <summary>
/// Minimal best-effort file logger. Alarms failing "silently" is the hardest kind of bug to
/// diagnose, so every tick, fire, and swallowed exception in the reminder pipeline gets a line
/// here. Never throws - a logging failure must never be the reason an alarm doesn't fire.
/// </summary>
public static class Logger
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RemindMe", "log.txt");

    private static readonly object Lock = new();

    public static void Log(string message)
    {
        try
        {
            lock (Lock)
            {
                var dir = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(LogPath, line);

                // Keep the log from growing forever.
                var info = new FileInfo(LogPath);
                if (info.Exists && info.Length > 2 * 1024 * 1024)
                {
                    var lines = File.ReadAllLines(LogPath);
                    var keep = lines.Length > 2000 ? lines[^2000..] : lines;
                    File.WriteAllLines(LogPath, keep);
                }
            }
        }
        catch
        {
            // Logging must never be able to crash or block the app.
        }
    }

    public static void LogException(string context, Exception ex) => Log($"ERROR in {context}: {ex}");
}
