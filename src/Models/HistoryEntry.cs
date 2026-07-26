using System;
using System.Text.Json.Serialization;

namespace RemindMe.Models;

public enum HistoryEventType
{
    Added,
    Paid,
    Unpaid,
    Deleted
}

/// <summary>A single logged event about a bill, shown in the History section.</summary>
public class HistoryEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BillId { get; set; }
    public string BillName { get; set; } = string.Empty;
    public BillCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public DateTime EventDate { get; set; } = DateTime.Now;
    public HistoryEventType EventType { get; set; }

    [JsonIgnore]
    public string EventLabel => EventType switch
    {
        HistoryEventType.Added => "Added",
        HistoryEventType.Paid => "Marked Paid",
        HistoryEventType.Unpaid => "Marked Unpaid",
        HistoryEventType.Deleted => "Deleted",
        _ => EventType.ToString()
    };
}
