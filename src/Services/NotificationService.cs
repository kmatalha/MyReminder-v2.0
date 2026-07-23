using System;
using Microsoft.Toolkit.Uwp.Notifications;
using RemindMe.Models;

namespace RemindMe.Services;

public class NotificationService
{
    /// <summary>Fired when the toast body or a toast button is activated. action is "pay", "snooze", or null (body tap).</summary>
    public event Action<Guid, string?>? ToastActionInvoked;

    public NotificationService()
    {
        ToastNotificationManagerCompat.OnActivated += toastArgs =>
        {
            var args = ToastArguments.Parse(toastArgs.Argument);
            if (args.Contains("billId") && Guid.TryParse(args["billId"], out var billId))
            {
                var action = args.Contains("action") ? args["action"] : null;
                ToastActionInvoked?.Invoke(billId, action);
            }
        };
    }

    public void ShowReminder(Bill bill, NotificationStyle style, bool overdue, string? customSoundPath = null)
    {
        var daysLeft = (bill.DueDate.Date - DateTime.Today).Days;
        var subtitle = overdue
            ? $"Overdue by {Math.Abs(daysLeft)} day{(Math.Abs(daysLeft) == 1 ? "" : "s")}"
            : daysLeft == 0
                ? "Due today"
                : $"Due in {daysLeft} day{(daysLeft == 1 ? "" : "s")}";

        var builder = new ToastContentBuilder()
            .AddArgument("billId", bill.Id.ToString())
            .AddText($"{(overdue ? "⚠ " : "")}{bill.Name}")
            .AddText(subtitle);

        if (!string.IsNullOrWhiteSpace(bill.Description))
        {
            builder.AddText(bill.Description);
        }

        builder.AddButton(new ToastButton("Mark as Paid", new ToastArguments()
                .Add("action", "pay")
                .Add("billId", bill.Id.ToString())
                .ToString()))
            .AddButton(new ToastButton("Snooze 1 Day", new ToastArguments()
                .Add("action", "snooze")
                .Add("billId", bill.Id.ToString())
                .ToString()));

        switch (style)
        {
            case NotificationStyle.Popup:
                builder.SetToastDuration(ToastDuration.Long);
                ApplyCustomAudio(builder, customSoundPath);
                break;
            case NotificationStyle.Subtle:
                builder.SetToastDuration(ToastDuration.Short)
                       .AddAudio(new Uri("ms-winsoundevent:Notification.Silent"));
                break;
            case NotificationStyle.Banner:
            default:
                ApplyCustomAudio(builder, customSoundPath);
                break;
        }

        builder.Show();
    }

    /// <summary>Points the toast's audio at the user's uploaded sound file, if one is set and still on disk.</summary>
    private static void ApplyCustomAudio(ToastContentBuilder builder, string? customSoundPath)
    {
        if (string.IsNullOrWhiteSpace(customSoundPath) || !System.IO.File.Exists(customSoundPath)) return;
        builder.AddAudio(new Uri(customSoundPath));
    }

    public static void ClearAll() => ToastNotificationManagerCompat.History.Clear();
}
