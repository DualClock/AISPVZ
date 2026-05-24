using System.Diagnostics;
using System.Windows;
using AISPVZ.Services;
using Microsoft.Win32;

namespace AISPVZ.Views;

public partial class SettingsWindow : Window
{
    private readonly ReferenceService _referenceService;
    private readonly DatabaseService _databaseService;

    public SettingsWindow()
    {
        InitializeComponent();
        _referenceService = new ReferenceService();
        _databaseService = new DatabaseService();
    }

    private async void CreateBackupClick(object sender, RoutedEventArgs e)
    {
        var success = await _databaseService.BackupDatabaseAsync();
        if (success)
        {
            var backupsPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AISPVZ", "Backups");
            MessageBox.Show($"Резервная копия создана!\n\nПуть: {backupsPath}", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("Ошибка создания резервной копии", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenBackupsFolderClick(object sender, RoutedEventArgs e)
    {
        var backupsPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AISPVZ", "Backups");
        System.IO.Directory.CreateDirectory(backupsPath);
        Process.Start("explorer.exe", backupsPath);
    }

    private async void SaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            await _referenceService.SetSettingAsync("PVZName", PvzNameBox.Text);
            await _referenceService.SetSettingAsync("PVZAddress", AddressBox.Text);
            await _referenceService.SetSettingAsync("MaxStorageDays", MaxDaysBox.Text);
            await _referenceService.SetSettingAsync("ReminderHoursBefore", ReminderHoursBox.Text);

            MessageBox.Show("Настройки сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}