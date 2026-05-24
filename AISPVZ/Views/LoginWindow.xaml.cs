using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AISPVZ.ViewModels;
using AISPVZ.Models;
using AISPVZ.Services;
using AISPVZ.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AISPVZ.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
        _viewModel = new LoginViewModel();
        DataContext = _viewModel;

        _viewModel.LoginCompleted += OnLoginCompleted;

        Loaded += async (s, e) =>
        {
            await InitializeDatabaseAsync();
            LoginBox.Focus();
        };

        PasswordBox.PreviewKeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter)
            {
                _viewModel.Password = PasswordBox.Password;
                _viewModel.LoginCommand.Execute(null);
            }
        };
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            var dbService = new DatabaseService();
            await dbService.InitializeDatabaseAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;
    }

    private void OnLoginCompleted(bool success, Employee employee)
    {
        if (success)
        {
            App.SetCurrentOperator(employee);
            var mainWindow = new MainWindow(employee);
            mainWindow.Show();
            Close();
        }
    }
}