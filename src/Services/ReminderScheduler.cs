using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using RemindMe.Models;

namespace RemindMe.Services;

/// <summary>
/// Ticks periodically (default: every 15 minutes) and, once per calendar day per bill,
/// fires a toast for any bill that is currently due for a reminder.
/// </summary>
public class ReminderScheduler
{
    private readonly DispatcherTimer _timer;
    private readonly NotificationService _notificationService;
    private readonly StorageService _storageService;
    private readonly Func<IEnumerable<Bill>> _getBills;
    private readonly Func<AppSettings> _getSettings;

    public ReminderScheduler(
        NotificationService notificationService,
        StorageService storageService,
        Func<IEnumerable<Bill>> getBills,
        Func<AppSettings> getSettings)
    {
        _notificationService = notificationService;
        _storageService = storageService;
        _getBills = getBills;
        _getSettings = getSettings;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(15) };
        _timer.Tick += (_, _) => CheckNow();
    }

    public void Start()
    {
        CheckNow();
        _timer.Start();
    }

    public void Stop() => _timer.Stop();

    /// <summary>Runs an immediate pass over all bills, e.g. right after app launch.</summary>
    public void CheckNow()
    {
        var settings = _getSettings();
        var now = DateTime.Now;
        var today = DateTime.Today;
        var bills = _getBills().ToList();
        var changed = false;

        foreach (var bill in bills)
        {
            var dueForReminder = bill.IsDueForReminder(today);
            var overdue = bill.IsOverdue(today);

            if (!dueForReminder && !overdue) continue;
            if (!bill.IsTimeToRing(now)) continue;
            if (bill.LastNotifiedDate?.Date == today) continue;

            var style = bill.NotificationStyleOverride ?? settings.DefaultNotificationStyle;
            _notificationService.ShowReminder(bill, style, overdue, settings.CustomAlarmSoundPath);
            bill.LastNotifiedDate = today;
            changed = true;
        }

        if (changed)
        {
            _storageService.SaveBills(bills);
        }
    }
}
