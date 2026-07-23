using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace RemindMe.Models;

public class Bill : INotifyPropertyChanged
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public BillCategory Category { get; set; } = BillCategory.Other;

    public decimal Amount { get; set; }

    private DateTime _dueDate;
    public DateTime DueDate
    {
        get => _dueDate;
        set => SetField(ref _dueDate, value.Date);
    }

    private DateTime _reminderStartDate;
    public DateTime ReminderStartDate
    {
        get => _reminderStartDate;
        set => SetField(ref _reminderStartDate, value.Date);
    }

    public RecurrenceType Recurrence { get; set; } = RecurrenceType.OneTime;

    private bool _isPaid;
    public bool IsPaid
    {
        get => _isPaid;
        set => SetField(ref _isPaid, value);
    }

    public DateTime? LastPaidDate { get; set; }

    /// <summary>If set and in the future, reminders are suppressed until this date/time.</summary>
    public DateTime? SnoozeUntil { get; set; }

    /// <summary>Date the last toast was actually shown for the current billing cycle (prevents duplicate daily toasts).</summary>
    public DateTime? LastNotifiedDate { get; set; }

    public NotificationStyle? NotificationStyleOverride { get; set; }

    /// <summary>
    /// True when today falls within [ReminderStartDate, DueDate], the bill is unpaid,
    /// and it isn't currently snoozed.
    /// </summary>
    public bool IsDueForReminder(DateTime today)
    {
        if (IsPaid) return false;
        if (SnoozeUntil.HasValue && today.Date < SnoozeUntil.Value.Date) return false;
        return today.Date >= ReminderStartDate.Date;
    }

    public bool IsOverdue(DateTime today) => !IsPaid && today.Date > DueDate.Date;

    [JsonIgnore]
    public string StatusText
    {
        get
        {
            var today = DateTime.Today;
            if (IsPaid) return $"Paid{(LastPaidDate.HasValue ? $" on {LastPaidDate:MMM d}" : "")}";
            if (IsOverdue(today))
            {
                var days = (today - DueDate.Date).Days;
                return $"Overdue by {days} day{(days == 1 ? "" : "s")}";
            }
            var left = (DueDate.Date - today).Days;
            if (left == 0) return "Due today";
            if (left < 0) return "Overdue";
            return $"Due in {left} day{(left == 1 ? "" : "s")} ({DueDate:MMM d})";
        }
    }

    /// <summary>
    /// Marks the bill paid. For recurring bills, immediately rolls the due date and
    /// reminder start date forward to the next cycle and resets the paid flag,
    /// so the bill re-arms itself for next time.
    /// </summary>
    public void MarkPaid(DateTime paidOn)
    {
        LastPaidDate = paidOn.Date;
        SnoozeUntil = null;
        LastNotifiedDate = null;

        if (Recurrence == RecurrenceType.OneTime)
        {
            IsPaid = true;
            return;
        }

        var offset = DueDate - ReminderStartDate;
        DueDate = Recurrence == RecurrenceType.Monthly
            ? DueDate.AddMonths(1)
            : DueDate.AddYears(1);
        ReminderStartDate = DueDate - offset;
        IsPaid = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
