using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using Microsoft.Win32;
using RemindMe.Models;

namespace RemindMe.Services;

/// <summary>
/// Ticks periodically (default: every 1 minute) and, once per calendar day per bill,
/// fires an alarm for any bill that is currently due for a reminder.
///
/// Design rule: the in-app alarm (BillAlarm event -> AlarmWindow) is the ONE thing that is
/// guaranteed to happen. The Windows toast is a nice-to-have on top of it. Nothing about the
/// toast pipeline (missing AUMID, no notification permission, an unactivated shortcut, a WinRT
/// exception) is allowed to prevent BillAlarm from firing - that was the bug that made alarms
/// go silent with no error and no window.
/// </summary>
public class ReminderScheduler
{
    private readonly DispatcherTimer _timer;
    private readonly NotificationService _notificationService;
    private readonly StorageService _storageService;
    private readonly Func<IEnumerable<Bill>> _getBills;
    private readonly Func<AppSettings> _getSettings;

    /// <summary>Fired for every bill whose alarm goes off. The subscriber shows the guaranteed
    /// in-app alarm window - it must not depend on the toast having succeeded.</summary>
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

        // Was 15 minutes - tightened to 1 so a bill armed for e.g. 9:00 rings within a minute
        // of 9:00 instead of possibly waiting up to a quarter hour.
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timer.Tick += (_, _) => SafeCheckNow();

        // Laptops/desktops that sleep through a scheduled reminder time will otherwise not see
        // it again until the next timer tick after waking - check immediately on resume instead.
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume) SafeCheckNow();
    }

    public void Start()
    {
        SafeCheckNow();
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }

    private void SafeCheckNow()
    {
        try
        {
            CheckNow();
        }
        catch (Exception ex)
        {
            // A bug here must never silently take down the whole reminder pipeline (or the app,
            // if this ran unguarded inside a DispatcherTimer.Tick).
            Logger.LogException(nameof(ReminderScheduler) + "." + nameof(CheckNow), ex);
        }
    }

    /// <summary>Runs an immediate pass over all bills, e.g. right after app launch.</summary>
    public void CheckNow()
    {
        var settings = _getSettings();
        var now = DateTime.Now;
        var today = DateTime.Today;
        var bills = _getBills().ToList();
        var changed = false;

        Logger.Log($"CheckNow: scanning {bills.Count} bill(s) at {now:HH:mm}");

        foreach (var bill in bills)
        {
            try
            {
                var dueForReminder = bill.IsDueForReminder(today);
                var overdue = bill.IsOverdue(today);

                if (!dueForReminder && !overdue) continue;
                if (!bill.IsTimeToRing(now)) continue;
                if (bill.LastNotifiedDate?.Date == today) continue;

                Logger.Log($"Firing alarm for '{bill.Name}' (overdue={overdue})");

                // Guaranteed alarm first - this must happen even if the toast below throws.
                bill.LastNotifiedDate = today;
                changed = true;
                BillAlarm?.Invoke(bill, overdue);

                // Best-effort toast on top. Fully isolated: nothing it does (including exceptions
                // thrown while building the toast content, not just while showing it) can stop
                // the alarm above or the next bill in this loop from being processed.
                try
                {
                    var style = bill.NotificationStyleOverride ?? settings.DefaultNotificationStyle;
                    _notificationService.ShowReminder(bill, style, overdue, settings.CustomAlarmSoundPath);
                }
                catch (Exception toastEx)
                {
                    Logger.LogException($"toast for '{bill.Name}'", toastEx);
                }
            }
            catch (Exception billEx)
            {
                // One bad bill (bad dates, etc.) must not stop the rest from being checked.
                Logger.LogException($"checking bill '{bill.Name}'", billEx);
            }
        }

        if (changed)
        {
            _storageService.SaveBills(bills);
        }
    }
}
