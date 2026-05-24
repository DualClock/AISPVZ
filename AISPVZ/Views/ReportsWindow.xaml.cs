using System.Windows;
using AISPVZ.ViewModels;

namespace AISPVZ.Views;

public partial class ReportsWindow : Window
{
    private readonly ReportsViewModel _viewModel;

    public ReportsWindow(int employeeId, int shiftId)
    {
        InitializeComponent();
        _viewModel = new ReportsViewModel();
        _viewModel.SetContext(employeeId, shiftId);
        DataContext = _viewModel;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadDashboardCommand.ExecuteAsync(null);
    }
}
