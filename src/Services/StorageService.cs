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
    }

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
}
