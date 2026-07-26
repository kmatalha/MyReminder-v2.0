# RemindMe — Bill & Subscription Reminder App for Windows

A minimalist, modern bill reminder app for Windows. Set a due date and a reminder
window, get daily toast notifications until you mark the bill paid, and track
everything — utilities, subscriptions, fees — from a clean dashboard.

## Features

- **Reminder window logic** — set a due date and a reminder *start* date; RemindMe
  sends one toast notification per day from the start date until the due date.
- **Mark as paid** stops reminders immediately. Recurring bills (monthly/yearly)
  automatically roll forward to the next cycle and re-arm themselves.
- **One-time or recurring** bills (none / monthly / yearly).
- **Runs at Windows startup** (toggle in the sidebar), minimizes to the system tray
  instead of closing.
- **Actionable toasts** — "Mark as Paid" and "Snooze 1 Day" buttons right on the
  notification, no need to open the app.
- **Customizable notification style** — Popup (long + sound), Banner (default),
  or Subtle (short + silent) — per app default or per bill.
- **Dashboard** with color-coded status (upcoming / due soon / overdue / paid),
  category icons, and a monthly paid-vs-unpaid summary.
- **Light (pastel) and dark themes**, switchable instantly.
- **Smooth animations** when marking a bill paid.

## Tech Stack

- .NET 8, WPF (Windows only)
- `Microsoft.Toolkit.Uwp.Notifications` for native Windows toast notifications
- Local JSON storage in `%AppData%\RemindMe\` (no account or cloud needed)
- Single-file, self-contained publish — the output `.exe` runs on any Windows 10/11
  64-bit machine with no separate .NET install required

## Building locally

```bash
dotnet restore src/RemindMe.csproj
dotnet build src/RemindMe.csproj -c Release
```

To produce the same portable `.exe` the CI pipeline generates:

```bash
dotnet publish src/RemindMe.csproj -c Release -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true \
  -o publish
```

The resulting `publish/RemindMe.exe` is portable — copy it anywhere and run it,
no installer required.

## Continuous Integration

`.github/workflows/build.yml` runs on every push/PR to `main` (and can be run
manually via "Run workflow"):

1. Checks out the repo on a `windows-latest` runner.
2. Installs the .NET 8 SDK.
3. Restores dependencies.
4. Publishes a self-contained, single-file `win-x64` `.exe`.
5. Uploads it as a downloadable workflow artifact named **RemindMe-portable-exe**.

Grab the built `.exe` from the **Actions** tab → the latest run → **Artifacts**.

## Project layout

```
src/
  Models/        Bill, BillCategory, RecurrenceType, NotificationStyle, AppSettings
  Services/      StorageService (JSON), NotificationService (toasts),
                 ReminderScheduler (daily check loop), StartupService (registry
                 Run key), ShortcutHelper (AUMID shortcut for toast support)
  Views/         AddEditBillWindow (add/edit dialog)
  Converters/    XAML value converters for icons/status colors
  Themes/        Light/Dark resource dictionaries + shared control styles
  MainWindow.*   Dashboard UI
.github/workflows/build.yml   CI pipeline
```

## Notes

- Notifications rely on a Start Menu shortcut stamped with an AppUserModelID,
  which RemindMe creates silently on first launch (standard requirement for
  unpackaged Win32 apps to raise toast notifications).
- All data stays local in `%AppData%\RemindMe\bills.json` and `settings.json`.
