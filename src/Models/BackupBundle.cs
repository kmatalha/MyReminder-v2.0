using System.Collections.Generic;

namespace RemindMe.Models;

/// <summary>Everything needed to restore the app's data from a single backup file.</summary>
public class BackupBundle
{
    public int BackupVersion { get; set; } = 1;
    public List<Bill> Bills { get; set; } = new();
    public List<HistoryEntry> History { get; set; } = new();
    public AppSettings Settings { get; set; } = new();
}
