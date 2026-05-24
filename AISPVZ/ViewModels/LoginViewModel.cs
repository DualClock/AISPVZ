using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Services;
using System.Security.Authentication;

namespace AISPVZ.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty]
    private string _login = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action<bool, Models.Employee>? LoginCompleted;

    public LoginViewModel()
    {
        _authService = new AuthService();
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Введите логин и пароль";
            return;
        }

        IsLoading = true;
        ErrorMessage = "";

        try
        {
            var employee = await _authService.LoginAsync(Login, Password);
            LoginCompleted?.Invoke(true, employee);
        }
        catch (AuthenticationException ex)
        {
            ErrorMessage = ex.Message;
            LoginCompleted?.Invoke(false, null!);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ошибка авторизации: " + ex.Message;
            LoginCompleted?.Invoke(false, null!);
        }
        finally
        {
            IsLoading = false;
        }
    }
}