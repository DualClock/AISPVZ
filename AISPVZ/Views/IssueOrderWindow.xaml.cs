using System.Windows;
using System.Windows.Input;
using AISPVZ.ViewModels;

namespace AISPVZ.Views;

public partial class IssueOrderWindow : Window
{
    private readonly IssueOrderViewModel _viewModel;
    private readonly bool _isReturnMode;

    public IssueOrderWindow(int employeeId, int shiftId, bool isAdmin = false, Models.Order? order = null, bool isReturnMode = false, Models.Employee? currentEmployee = null)
    {
        InitializeComponent();

        _isReturnMode = isReturnMode;
        _viewModel = new IssueOrderViewModel();
        DataContext = _viewModel;

        Title = isReturnMode ? "Возврат заказа" : "Выдача заказа";

        _viewModel.OperationCompleted += success =>
        {
            if (success)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Операция выполнена успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                });
            }
        };

        Loaded += async (s, e) =>
        {
            await _viewModel.InitializeAsync(employeeId, shiftId, isAdmin, isReturnMode, currentEmployee);
            if (order != null)
            {
                _viewModel.Barcode = order.Barcode;
                await _viewModel.SearchByBarcodeCommand.ExecuteAsync(null);
            }
            BarcodeBox.Focus();
        };
    }

    private void BarcodeBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _ = _viewModel.SearchByBarcodeCommand.ExecuteAsync(null);
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}