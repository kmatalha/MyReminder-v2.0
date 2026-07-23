namespace RemindMe.Models;

public class AppSettings
{
    public bool DarkMode { get; set; } = false;
    public bool RunAtStartup { get; set; } = true;
    public NotificationStyle DefaultNotificationStyle { get; set; } = NotificationStyle.Banner;
    public string CheckTime { get; set; } = "09:00"; // HH:mm, daily reminder check time

    /// <summary>Absolute path to a copy of the user's uploaded alarm sound file (stored under app data). Null = use the default OS notification sound.</summary>
    public string? CustomAlarmSoundPath { get; set; }

    /// <summary>Original file name of the uploaded sound, kept for display purposes only.</summary>
    public string? CustomAlarmSoundName { get; set; }
}
