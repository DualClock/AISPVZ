using System.Windows;
using AISPVZ.ViewModels;

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
}