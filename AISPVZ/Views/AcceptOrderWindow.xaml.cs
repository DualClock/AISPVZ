using System.Windows;
using AISPVZ.ViewModels;

namespace AISPVZ.Views;

public partial class AcceptOrderWindow : Window
{
    private readonly AcceptOrderViewModel _viewModel;

    public AcceptOrderWindow()
    {
        InitializeComponent();

        _viewModel = new AcceptOrderViewModel();
        DataContext = _viewModel;

        _viewModel.OperationCompleted += OnOperationCompleted;

        Loaded += async (s, e) =>
        {
            await _viewModel.LoadDataAsync();
            BarcodeInput.Focus();
        };
    }

    private void OnOperationCompleted(bool success)
    {
        if (success)
        {
            DialogResult = true;
            Close();
        }
    }
}