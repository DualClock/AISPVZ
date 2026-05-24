using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AISPVZ.Models;
using AISPVZ.ViewModels;
using AISPVZ.Services;

namespace AISPVZ.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(Employee employee)
    {
        InitializeComponent();

        _viewModel = new MainViewModel();
        _viewModel.SetEmployee(employee);
        DataContext = _viewModel;

        _viewModel.NavigateRequested += OnNavigateRequested;
        _viewModel.OrderSelectedFromScan += OnOrderSelectedFromScan;

        Loaded += async (s, e) =>
        {
            await _viewModel.RefreshDashboardAsync();
            ScannerBox.Focus();
        };
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.F2)
        {
            _viewModel.NavigateToCommand.Execute("accept");
            e.Handled = true;
            return;
        }

        switch (e.Key)
        {
            case Key.F1:
                MessageBox.Show(
                    "Горячие клавиши:\n\nF2 - Приёмка заказа\nF3 - Выдача заказа\nF4 - Возврат\nEsc - Назад/отмена\nTab - Навигация по полям",
                    "Помощь", MessageBoxButton.OK, MessageBoxImage.Information);
                break;
            case Key.F3:
                _viewModel.NavigateToCommand.Execute("issue");
                e.Handled = true;
                break;
            case Key.F4:
                _viewModel.NavigateToCommand.Execute("return");
                e.Handled = true;
                break;
            case Key.Escape:
                NavigateToPage("dashboard");
                break;
        }
    }

    private void ScannerBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _viewModel.ProcessBarcodeScanCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.F2)
        {
            _viewModel.NavigateToCommand.Execute("accept");
            e.Handled = true;
        }
        else if (e.Key == Key.F3)
        {
            _viewModel.NavigateToCommand.Execute("issue");
            e.Handled = true;
        }
        else if (e.Key == Key.F4)
        {
            _viewModel.NavigateToCommand.Execute("return");
            e.Handled = true;
        }
    }

    private void OnNavigateRequested(string page)
    {
        NavigateToPage(page);
    }

    private void NavigateToPage(string page)
    {
        switch (page)
        {
            case "accept":
                var acceptWindow = new AcceptOrderWindow();
                acceptWindow.ShowDialog();
                _ = _viewModel.RefreshDashboardAsync();
                break;
            case "issue":
                var issueWindow = new IssueOrderWindow(_viewModel.CurrentEmployee.Id, _viewModel.CurrentShift?.Id ?? 0);
                issueWindow.ShowDialog();
                _ = _viewModel.RefreshDashboardAsync();
                break;
            case "return":
                var returnWindow = new IssueOrderWindow(_viewModel.CurrentEmployee.Id, _viewModel.CurrentShift?.Id ?? 0, isReturnMode: true);
                returnWindow.ShowDialog();
                _ = _viewModel.RefreshDashboardAsync();
                break;
            case "cells":
                var cellsWindow = new StorageCellsWindow();
                cellsWindow.ShowDialog();
                break;
            case "clients":
                var clientsWindow = new ClientsWindow();
                clientsWindow.ShowDialog();
                break;
            case "employees":
                var employeesWindow = new EmployeesWindow();
                employeesWindow.ShowDialog();
                break;
            case "reports":
                var reportsWindow = new ReportsWindow(_viewModel.CurrentEmployee.Id, _viewModel.CurrentShift?.Id ?? 0);
                reportsWindow.ShowDialog();
                break;
            case "settings":
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
                break;
            default:
                ScannerBox.Focus();
                break;
        }
    }

    private void OnOrderSelectedFromScan(Order order)
    {
        var issueWindow = new IssueOrderWindow(_viewModel.CurrentEmployee.Id, _viewModel.CurrentShift?.Id ?? 0, order);
        issueWindow.ShowDialog();
        _ = _viewModel.RefreshDashboardAsync();
    }
}