namespace RemindMe.Models;

public class AppSettings
{
    public bool DarkMode { get; set; } = false;
    public bool RunAtStartup { get; set; } = true;
    public NotificationStyle DefaultNotificationStyle { get; set; } = NotificationStyle.Banner;
    public string CheckTime { get; set; } = "09:00"; // HH:mm, daily reminder check time
}
