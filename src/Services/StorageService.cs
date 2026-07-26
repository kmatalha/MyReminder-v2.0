using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RemindMe.Models;

namespace RemindMe.Services;

public class StorageService
{
    private readonly string _dataDir;
    private readonly string _billsPath;
    private readonly string _settingsPath;
    private readonly string _historyPath;
    private readonly string _soundsDir;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        IncludeFields = false
    };

    public StorageService()
    {
        _dataDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RemindMe");
        Directory.CreateDirectory(_dataDir);
        _billsPath = Path.Combine(_dataDir, "bills.json");
        _settingsPath = Path.Combine(_dataDir, "settings.json");
        _historyPath = Path.Combine(_dataDir, "history.json");
        _soundsDir = Path.Combine(_dataDir, "sounds");
        Directory.CreateDirectory(_soundsDir);
    }

    // ----- Bills -----

    public List<Bill> LoadBills()
    {
        try
        {
            if (!File.Exists(_billsPath)) return new List<Bill>();
            var json = File.ReadAllText(_billsPath);
            return JsonSerializer.Deserialize<List<Bill>>(json, _jsonOptions) ?? new List<Bill>();
        }
        catch
        {
            // Corrupt file shouldn't crash the app; back it up and start fresh.
            try
            {
                if (File.Exists(_billsPath))
                    File.Copy(_billsPath, _billsPath + ".corrupt-" + DateTime.Now.Ticks, overwrite: true);
            }
            catch { /* best effort */ }
            return new List<Bill>();
        }
    }

    public void SaveBills(IEnumerable<Bill> bills)
    {
        var json = JsonSerializer.Serialize(bills, _jsonOptions);
        var tmp = _billsPath + ".tmp";
        File.WriteAllText(tmp, json);
        File.Copy(tmp, _billsPath, overwrite: true);
        File.Delete(tmp);
    }

    // ----- Settings -----

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath)) return new AppSettings();
            var json = File.ReadAllText(_settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, _jsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, _jsonOptions);
        File.WriteAllText(_settingsPath, json);
    }

    // ----- History -----

    public List<HistoryEntry> LoadHistory()
    {
        try
        {
            if (!File.Exists(_historyPath)) return new List<HistoryEntry>();
            var json = File.ReadAllText(_historyPath);
            return JsonSerializer.Deserialize<List<HistoryEntry>>(json, _jsonOptions) ?? new List<HistoryEntry>();
        }
        catch
        {
            return new List<HistoryEntry>();
        }
    }

    public void SaveHistory(IEnumerable<HistoryEntry> history)
    {
        var json = JsonSerializer.Serialize(history, _jsonOptions);
        var tmp = _historyPath + ".tmp";
        File.WriteAllText(tmp, json);
        File.Copy(tmp, _historyPath, overwrite: true);
        File.Delete(tmp);
    }

    /// <summary>Loads history, inserts the new entry at the front (most recent first), and saves.</summary>
    public void AppendHistory(HistoryEntry entry)
    {
        var history = LoadHistory();
        history.Insert(0, entry);
        SaveHistory(history);
    }

    public void ClearHistory() => SaveHistory(new List<HistoryEntry>());

    // ----- Backup / Restore -----

    public void ExportBackup(string path, IEnumerable<Bill> bills, IEnumerable<HistoryEntry> history, AppSettings settings)
    {
        var bundle = new BackupBundle
        {
            Bills = new List<Bill>(bills),
            History = new List<HistoryEntry>(history),
            Settings = settings
        };
        var json = JsonSerializer.Serialize(bundle, _jsonOptions);
        File.WriteAllText(path, json);
    }

    /// <summary>Reads a backup file without applying it. Returns null if the file isn't a valid backup.</summary>
    public BackupBundle? ReadBackup(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<BackupBundle>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Overwrites current bills/history/settings with the contents of a backup bundle.</summary>
    public void ApplyBackup(BackupBundle bundle)
    {
        SaveBills(bundle.Bills);
        SaveHistory(bundle.History);
        SaveSettings(bundle.Settings);
    }

    // ----- Custom alarm sound -----

    /// <summary>
    /// Copies a user-chosen audio file into app-managed storage so the reminder keeps working
    /// even if the original file is later moved or deleted. Returns the stored absolute path.
    /// </summary>
    public string ImportAlarmSound(string sourceFilePath)
    {
        var ext = Path.GetExtension(sourceFilePath);
        var destName = $"alarm-{DateTime.Now.Ticks}{ext}";
        var destPath = Path.Combine(_soundsDir, destName);
        File.Copy(sourceFilePath, destPath, overwrite: true);
        return destPath;
    }
}
