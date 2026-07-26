using System;
using System.Globalization;
using System.Windows;
using RemindMe.Models;

namespace RemindMe.Views;

public partial class AddEditBillWindow : Window
{
    private readonly Bill? _editingBill;

    public Bill? ResultBill { get; private set; }

    public AddEditBillWindow()
    {
        InitializeComponent();
        DueDatePicker.SelectedDate = DateTime.Today.AddDays(30);
        ReminderStartPicker.SelectedDate = DateTime.Today.AddDays(23);
        CategoryCombo.SelectedIndex = 0;
        RecurrenceCombo.SelectedIndex = 0;
        NotificationStyleCombo.SelectedIndex = 0;

        PopulateTimeCombos();
        SetReminderTime(new TimeSpan(9, 0, 0));
    }

    public AddEditBillWindow(Bill billToEdit) : this()
    {
        _editingBill = billToEdit;
        TitleText.Text = "Edit Bill";

        NameBox.Text = billToEdit.Name;
        CategoryCombo.SelectedIndex = (int)billToEdit.Category;
        DescriptionBox.Text = billToEdit.Description;
        DueDatePicker.SelectedDate = billToEdit.DueDate;
        ReminderStartPicker.SelectedDate = billToEdit.ReminderStartDate;
        SetReminderTime(billToEdit.ReminderTime);
        RecurrenceCombo.SelectedIndex = (int)billToEdit.Recurrence;
        NotificationStyleCombo.SelectedIndex = billToEdit.NotificationStyleOverride switch
        {
            NotificationStyle.Popup => 1,
            NotificationStyle.Banner => 2,
            NotificationStyle.Subtle => 3,
            _ => 0
        };
    }

    private void PopulateTimeCombos()
    {
        for (var h = 0; h < 24; h++)
        {
            ReminderHourCombo.Items.Add(h.ToString("00", CultureInfo.InvariantCulture));
        }
        for (var m = 0; m < 60; m++)
        {
            ReminderMinuteCombo.Items.Add(m.ToString("00", CultureInfo.InvariantCulture));
        }
    }

    private void SetReminderTime(TimeSpan time)
    {
        ReminderHourCombo.SelectedIndex = Math.Clamp(time.Hours, 0, 23);
        ReminderMinuteCombo.SelectedIndex = Math.Clamp(time.Minutes, 0, 59);
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            ShowError("Please enter a bill name.");
            return;
        }
        if (DueDatePicker.SelectedDate is not DateTime dueDate)
        {
            ShowError("Please choose a due date.");
            return;
        }
        if (ReminderStartPicker.SelectedDate is not DateTime reminderStart)
        {
            ShowError("Please choose a reminder start date.");
            return;
        }
        if (reminderStart.Date > dueDate.Date)
        {
            ShowError("Reminder start date must be on or before the due date.");
            return;
        }
        if (ReminderHourCombo.SelectedItem is not string hourText ||
            ReminderMinuteCombo.SelectedItem is not string minuteText)
        {
            ShowError("Please choose an alarm time.");
            return;
        }

        var bill = _editingBill ?? new Bill();
        bill.Name = NameBox.Text.Trim();
        bill.Category = (BillCategory)CategoryCombo.SelectedIndex;
        bill.Description = DescriptionBox.Text?.Trim() ?? string.Empty;
        bill.DueDate = dueDate;
        bill.ReminderStartDate = reminderStart;
        bill.ReminderTime = new TimeSpan(
            int.Parse(hourText, CultureInfo.InvariantCulture),
            int.Parse(minuteText, CultureInfo.InvariantCulture),
            0);
        bill.Recurrence = (RecurrenceType)RecurrenceCombo.SelectedIndex;
        bill.NotificationStyleOverride = NotificationStyleCombo.SelectedIndex switch
        {
            1 => NotificationStyle.Popup,
            2 => NotificationStyle.Banner,
            3 => NotificationStyle.Subtle,
            _ => null
        };

        ResultBill = bill;
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

    private void ShowError(string message)
    {
        ErrorText.Text = message;
        ErrorText.Visibility = Visibility.Visible;
    }
}
