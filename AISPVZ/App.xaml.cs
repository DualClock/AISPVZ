using System.Windows;
using System.IO;
using AISPVZ.Services;
using AISPVZ.Models;

namespace AISPVZ;

public partial class App : Application
{
    private static Employee? _currentOperator;
    public static Employee? CurrentOperator => _currentOperator;

    public static void SetCurrentOperator(Employee? emp) => _currentOperator = emp;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Handle unhandled exceptions
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogException(ex);
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogException(args.Exception);
            args.Handled = true;
        };

        // Initialize database (create if not exists, seed with data)
        var dbService = new DatabaseService();
        await dbService.InitializeDatabaseAsync();

        // Auto-backup check
        await CheckAndPerformAutoBackupAsync();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        // Create auto-backup on exit if enabled
        await PerformAutoBackupAsync();
        base.OnExit(e);
    }

    private async Task CheckAndPerformAutoBackupAsync()
    {
        try
        {
            var dbService = new DatabaseService();
            var refService = new ReferenceService();

            var autoBackupEnabled = await refService.GetSettingAsync("AutoBackupEnabled");
            if (autoBackupEnabled?.ToLower() != "true") return;

            var lastBackupStr = await refService.GetSettingAsync("LastBackupDate");
            if (!string.IsNullOrEmpty(lastBackupStr) && DateTime.TryParse(lastBackupStr, out var lastBackup))
            {
                if ((DateTime.Now - lastBackup).TotalHours < 24) return;
            }

            await dbService.BackupDatabaseAsync();
            await dbService.UpdateLastBackupTimeAsync();
        }
        catch
        {
            // Silent fail for auto-backup
        }
    }

    private async Task PerformAutoBackupAsync()
    {
        try
        {
            var dbService = new DatabaseService();
            var refService = new ReferenceService();

            var autoBackupEnabled = await refService.GetSettingAsync("AutoBackupEnabled");
            if (autoBackupEnabled?.ToLower() != "true") return;

            await dbService.BackupDatabaseAsync();
            await dbService.UpdateLastBackupTimeAsync();
        }
        catch
        {
            // Silent fail for auto-backup
        }
    }

    private void LogException(Exception? ex)
    {
        if (ex == null) return;

        try
        {
            var logFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AISPVZ", "Logs");
            Directory.CreateDirectory(logFolder);

            var logFile = Path.Combine(logFolder, $"crash_{DateTime.Now:yyyyMMdd_HHmmss}.log");
            File.WriteAllText(logFile, $"{DateTime.Now}\n{ex}\n\n{ex.StackTrace}");
        }
        catch
        {
            // Silent fail
        }
    }
}