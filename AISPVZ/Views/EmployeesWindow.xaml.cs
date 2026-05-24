using System.Windows;
using AISPVZ.ViewModels;
using AISPVZ.Services;
using Microsoft.Win32;

namespace AISPVZ.Views;

public partial class EmployeesWindow : Window
{
    private readonly EmployeesViewModel _viewModel;

    public EmployeesWindow()
    {
        InitializeComponent();
        _viewModel = new EmployeesViewModel();
        DataContext = _viewModel;

        Loaded += async (s, e) => await _viewModel.LoadEmployeesCommand.ExecuteAsync(null);
    }

    private void ExportExcelClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            FileName = $"employees_{DateTime.Now:yyyyMMdd}"
        };
        if (dialog.ShowDialog() == true)
        {
            var export = new ExportService();
            export.ExportToExcel(_viewModel.Employees, dialog.FileName, "Сотрудники");
            MessageBox.Show("Экспорт завершён!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}