namespace RemindMe.Models;

/// <summary>
/// Controls how intrusive the daily toast reminder feels.
/// Popup   = long-duration toast that stays until dismissed, with sound.
/// Banner  = standard Windows toast banner (default OS behavior).
/// Subtle  = short, silent toast that disappears quickly.
/// </summary>
public enum NotificationStyle
{
    Popup,
    Banner,
    Subtle
}
