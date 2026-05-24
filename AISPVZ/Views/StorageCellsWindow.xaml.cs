using System.Windows;
using AISPVZ.ViewModels;
using Microsoft.Win32;
using AISPVZ.Services;

namespace AISPVZ.Views;

public partial class StorageCellsWindow : Window
{
    private readonly StorageCellsViewModel _viewModel;

    public StorageCellsWindow()
    {
        InitializeComponent();
        _viewModel = new StorageCellsViewModel();
        DataContext = _viewModel;

        Loaded += async (s, e) => await _viewModel.LoadCellsCommand.ExecuteAsync(null);
    }

    private async void ExportClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx",
            FileName = $"cells_export_{DateTime.Now:yyyyMMdd}"
        };

        if (dialog.ShowDialog() == true)
        {
            var export = new ExportService();
            if (dialog.FileName.EndsWith(".csv"))
                await export.ExportToCsvAsync(_viewModel.Cells, dialog.FileName);
            else
                export.ExportToExcel(_viewModel.Cells, dialog.FileName, "Ячейки");

            MessageBox.Show("Экспорт завершён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}