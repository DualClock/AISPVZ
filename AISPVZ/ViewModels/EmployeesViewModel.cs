using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;

namespace AISPVZ.ViewModels;

public partial class EmployeesViewModel : ObservableObject
{
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private ObservableCollection<Employee> _employees = new();

    [ObservableProperty]
    private Employee? _selectedEmployee;

    [ObservableProperty]
    private string _fullName = "";

    [ObservableProperty]
    private string _login = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private EmployeeRole _selectedRole = EmployeeRole.Operator;

    [ObservableProperty]
    private Dictionary<EmployeeRole, string> _roleOptions = new()
    {
        { EmployeeRole.Operator, "Оператор" },
        { EmployeeRole.Admin, "Администратор" }
    };

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _statusMessage = "";

    public event Action? DataChanged;

    public EmployeesViewModel()
    {
        _referenceService = new ReferenceService();
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        var emps = await _referenceService.GetAllEmployeesAsync();
        Employees = new ObservableCollection<Employee>(emps);
    }

    [RelayCommand]
    private void StartAdd()
    {
        SelectedEmployee = null;
        FullName = "";
        Login = "";
        Password = "";
        SelectedRole = EmployeeRole.Operator;
        IsEditing = true;
    }

    [RelayCommand]
    private void StartEdit()
    {
        if (SelectedEmployee == null) return;
        FullName = SelectedEmployee.FullName;
        Login = SelectedEmployee.Login;
        Password = "";
        SelectedRole = SelectedEmployee.Role;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveEmployeeAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Login))
        {
            StatusMessage = "Заполните обязательные поля";
            return;
        }

        try
        {
            if (SelectedEmployee != null)
            {
                SelectedEmployee.FullName = FullName;
                SelectedEmployee.Login = Login;
                SelectedEmployee.Role = SelectedRole;
                if (!string.IsNullOrWhiteSpace(Password))
                    SelectedEmployee.PasswordHash = Password;
                await _referenceService.UpdateEmployeeAsync(SelectedEmployee);
                StatusMessage = "Сотрудник обновлён";
            }
            else
            {
                var emp = new Employee
                {
                    FullName = FullName,
                    Login = Login,
                    PasswordHash = string.IsNullOrWhiteSpace(Password) ? "password123" : Password,
                    Role = SelectedRole,
                    IsActive = true
                };
                await _referenceService.CreateEmployeeAsync(emp);
                StatusMessage = "Сотрудник добавлен";
            }

            IsEditing = false;
            await LoadEmployeesAsync();
            DataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task DeactivateEmployeeAsync()
    {
        if (SelectedEmployee == null) return;

        try
        {
            await _referenceService.DeactivateEmployeeAsync(SelectedEmployee.Id);
            StatusMessage = "Сотрудник деактивирован";
            await LoadEmployeesAsync();
            DataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }
}