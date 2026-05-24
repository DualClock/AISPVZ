using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace AISPVZ.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly ShiftService _shiftService;
    private readonly AuthService _authService;
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private Employee _currentEmployee = null!;

    [ObservableProperty]
    private Shift? _currentShift;

    [ObservableProperty]
    private bool _hasOpenShift;

    [ObservableProperty]
    private DashboardStats _stats = new();

    [ObservableProperty]
    private ObservableCollection<Order> _expiringOrders = new();

    [ObservableProperty]
    private ObservableCollection<Order> _overdueOrders = new();

    [ObservableProperty]
    private ObservableCollection<Order> _inStorageOrders = new();

    [ObservableProperty]
    private string _barcodeScan = "";

    [ObservableProperty]
    private string _phoneSearch = "";

    [ObservableProperty]
    private int _overdueCount;

    [ObservableProperty]
    private string _searchStatusMessage = "";

    [ObservableProperty]
    private bool _isAdmin;

    // For navigation
    public event Action<string>? NavigateRequested;
    public event Action<Order>? OrderSelectedFromScan;
    public event Action<int>? OverdueNotificationRequested;
    public event Action<List<Order>>? PhoneSearchResultsReceived;

    public MainViewModel()
    {
        _orderService = new OrderService();
        _shiftService = new ShiftService();
        _authService = new AuthService();
        _referenceService = new ReferenceService();
    }

    public void SetEmployee(Employee employee)
    {
        CurrentEmployee = employee;
        IsAdmin = employee.Role == EmployeeRole.Admin;
        _ = LoadShiftStatusAsync();
    }

    private async Task LoadShiftStatusAsync()
    {
        try
        {
            CurrentShift = await _shiftService.GetOpenShiftAsync(CurrentEmployee.Id);
            HasOpenShift = CurrentShift != null;
            if (HasOpenShift)
            {
                var overdue = await _shiftService.GetOverdueOrdersCountAsync();
                OverdueCount = overdue;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки статуса смены: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    public async Task RefreshDashboardAsync()
    {
        try
        {
            SearchStatusMessage = "";
            var result = await _orderService.GetDashboardDataAsync();
            Stats = result.stats;
            ExpiringOrders = new ObservableCollection<Order>(result.expiringOrders);
            OverdueOrders = new ObservableCollection<Order>(result.overdueOrders);

            var allInStorage = await _orderService.GetInStorageOrdersAsync();
            InStorageOrders = new ObservableCollection<Order>(allInStorage);
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null)
                msg += "\nInner: " + ex.InnerException.Message;
            MessageBox.Show("Ошибка обновления дашборда:\n" + msg, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task OpenShiftAsync()
    {
        try
        {
            if (HasOpenShift) return;
            CurrentShift = await _shiftService.OpenShiftAsync(CurrentEmployee.Id);
            HasOpenShift = true;

            var overdueCount = await _shiftService.GetOverdueOrdersCountAsync();
            if (overdueCount > 0)
            {
                OverdueNotificationRequested?.Invoke(overdueCount);
            }

            await RefreshDashboardAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка открытия смены: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task CloseShiftAsync()
    {
        try
        {
            if (!HasOpenShift || CurrentShift == null) return;
            await _shiftService.CloseShiftAsync(CurrentShift.Id);
            CurrentShift = null;
            HasOpenShift = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка закрытия смены: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task ProcessBarcodeScanAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeScan)) return;
        SearchStatusMessage = "";

        try
        {
            var order = await _orderService.GetByBarcodeAsync(BarcodeScan);
            if (order != null)
            {
                OrderSelectedFromScan?.Invoke(order);
            }
            else
            {
                SearchStatusMessage = "Заказ не найден";
            }
        }
        catch (Exception ex)
        {
            SearchStatusMessage = "Ошибка: " + ex.Message;
        }
        finally
        {
            BarcodeScan = "";
        }
    }

    [RelayCommand]
    private async Task ProcessPhoneSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(PhoneSearch)) return;
        SearchStatusMessage = "";

        try
        {
            var orders = await _orderService.SearchByPhoneAsync(PhoneSearch);
            if (orders.Count == 1)
            {
                var order = await _orderService.GetByIdAsync(orders[0].Id);
                if (order != null)
                    OrderSelectedFromScan?.Invoke(order);
            }
            else if (orders.Count > 1)
            {
                PhoneSearchResultsReceived?.Invoke(orders);
            }
            else
            {
                SearchStatusMessage = "Клиент не найден";
            }
        }
        catch (Exception ex)
        {
            SearchStatusMessage = "Ошибка: " + ex.Message;
        }
        finally
        {
            PhoneSearch = "";
        }
    }

    [RelayCommand]
    private void NavigateTo(string page)
    {
        NavigateRequested?.Invoke(page);
    }

    [RelayCommand]
    private void Logout()
    {
        NavigateRequested?.Invoke("login");
    }
}
