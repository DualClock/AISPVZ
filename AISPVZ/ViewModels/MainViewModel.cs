using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;

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
    private string _barcodeScan = "";

    [ObservableProperty]
    private int _overdueCount;

    [ObservableProperty]
    private bool _isAdmin;

    // For navigation
    public event Action<string>? NavigateRequested;
    public event Action<Order>? OrderSelectedFromScan;

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
        CurrentShift = await _shiftService.GetOpenShiftAsync(CurrentEmployee.Id);
        HasOpenShift = CurrentShift != null;
        if (HasOpenShift)
        {
            var overdue = await _shiftService.GetOverdueOrdersCountAsync();
            OverdueCount = overdue;
        }
    }

    [RelayCommand]
    public async Task RefreshDashboardAsync()
    {
        var result = await _orderService.GetDashboardDataAsync();
        Stats = result.stats;
        ExpiringOrders = new ObservableCollection<Order>(result.expiringOrders);
        OverdueOrders = new ObservableCollection<Order>(result.overdueOrders);
    }

    [RelayCommand]
    private async Task OpenShiftAsync()
    {
        if (HasOpenShift) return;
        CurrentShift = await _shiftService.OpenShiftAsync(CurrentEmployee.Id);
        HasOpenShift = true;
        await RefreshDashboardAsync();
    }

    [RelayCommand]
    private async Task CloseShiftAsync()
    {
        if (!HasOpenShift || CurrentShift == null) return;
        await _shiftService.CloseShiftAsync(CurrentShift.Id);
        CurrentShift = null;
        HasOpenShift = false;
    }

    [RelayCommand]
    private async Task ProcessBarcodeScanAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeScan)) return;

        var order = await _orderService.GetByBarcodeAsync(BarcodeScan);
        if (order != null)
        {
            OrderSelectedFromScan?.Invoke(order);
        }
        BarcodeScan = "";
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