using System.Windows;
using AISPVZ.ViewModels;
using Microsoft.Win32;
using AISPVZ.Services;

namespace AISPVZ.Views;

public partial class ClientsWindow : Window
{
    private readonly ClientsViewModel _viewModel;

    public ClientsWindow()
    {
        InitializeComponent();
        _viewModel = new ClientsViewModel();
        DataContext = _viewModel;

        Loaded += async (s, e) => await _viewModel.LoadClientsCommand.ExecuteAsync(null);
    }

    private async void ExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx",
            FileName = $"clients_export_{DateTime.Now:yyyyMMdd}"
        };

        if (dialog.ShowDialog() == true)
        {
            var export = new ExportService();
            if (dialog.FileName.EndsWith(".csv"))
                await export.ExportToCsvAsync(_viewModel.Clients, dialog.FileName);
            else
                export.ExportToExcel(_viewModel.Clients, dialog.FileName, "Клиенты");

            MessageBox.Show("Экспорт завершён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}