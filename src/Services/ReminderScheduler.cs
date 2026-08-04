using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using RemindMe.Models;

namespace RemindMe.Services;

/// <summary>
/// Ticks periodically (default: every 1 minute) and, once per calendar day per bill,
/// fires a toast for any bill that is currently due for a reminder.
/// </summary>
public class ReminderScheduler
{
    private readonly DispatcherTimer _timer;
    private readonly NotificationService _notificationService;
    private readonly StorageService _storageService;
    private readonly Func<IEnumerable<Bill>> _getBills;
    private readonly Func<AppSettings> _getSettings;

    /// <summary>Fired for every bill whose alarm goes off, in addition to the (best-effort) toast.
    /// The subscriber is expected to show something the user can't miss - e.g. an in-app alarm window.</summary>
    public event Action<Bill, bool>? BillAlarm;

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

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
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

        Logger.Log($"CheckNow: evaluating {bills.Count} bill(s) at {now:yyyy-MM-dd HH:mm:ss}.");

        foreach (var bill in bills)
        {
            try
            {
                var dueForReminder = bill.IsDueForReminder(today);
                var overdue = bill.IsOverdue(today);

                if (!dueForReminder && !overdue) continue;
                if (!bill.IsTimeToRing(now))
                {
                    Logger.Log($"CheckNow: \"{bill.Name}\" is due but alarm time {bill.ReminderTime} hasn't arrived yet.");
                    continue;
                }
                if (bill.LastNotifiedDate?.Date == today)
                {
                    Logger.Log($"CheckNow: \"{bill.Name}\" already notified today, skipping.");
                    continue;
                }

                Logger.Log($"CheckNow: firing alarm for \"{bill.Name}\" (overdue={overdue}).");

                var style = bill.NotificationStyleOverride ?? settings.DefaultNotificationStyle;
                _notificationService.ShowReminder(bill, style, overdue, settings.CustomAlarmSoundPath);
                BillAlarm?.Invoke(bill, overdue);
                bill.LastNotifiedDate = today;
                changed = true;
            }
            catch (Exception ex)
            {
                // A problem with one bill (bad data, a subscriber that throws, etc.) must never
                // stop the rest of the pass or crash the app.
                Logger.LogError($"CheckNow (bill \"{bill.Name}\")", ex);
            }
        }

        if (changed)
        {
            _storageService.SaveBills(bills);
        }
    }
}
