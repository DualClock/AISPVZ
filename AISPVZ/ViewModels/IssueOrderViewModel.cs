using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;

namespace AISPVZ.ViewModels;

public partial class IssueOrderViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly ShiftService _shiftService;
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private Order? _currentOrder;

    [ObservableProperty]
    private Order? _selectedOrder;

    [ObservableProperty]
    private ObservableCollection<Order> _activeOrders = new();

    [ObservableProperty]
    private string _barcode = "";

    [ObservableProperty]
    private bool _isOrderFound;

    [ObservableProperty]
    private bool _isOverdue;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _showConfirmation;

    [ObservableProperty]
    private IssueResult? _pendingResult;

    [ObservableProperty]
    private ObservableCollection<OrderItemCheckViewModel> _itemChecks = new();

    [ObservableProperty]
    private string _selectedReturnReason = "";

    [ObservableProperty]
    private ObservableCollection<string> _returnReasons = new();

    private int _currentShiftId;
    private int _currentEmployeeId;

    public event Action<bool>? OperationCompleted;

    public IssueOrderViewModel()
    {
        _orderService = new OrderService();
        _shiftService = new ShiftService();
        _referenceService = new ReferenceService();
    }

    public async Task InitializeAsync(int employeeId, int shiftId)
    {
        _currentEmployeeId = employeeId;
        _currentShiftId = shiftId;
        var reasons = await _referenceService.GetReturnReasonsAsync();
        ReturnReasons = new ObservableCollection<string>(reasons);
        await LoadActiveOrdersAsync();
    }

    private async Task LoadActiveOrdersAsync()
    {
        var orders = await _orderService.GetActiveOrdersAsync();
        ActiveOrders = new ObservableCollection<Order>(orders);
    }

    [RelayCommand]
    private async Task SearchByBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Barcode)) return;

        var order = await _orderService.GetByBarcodeAsync(Barcode);
        if (order == null)
        {
            StatusMessage = "Заказ не найден";
            IsOrderFound = false;
            return;
        }

        CurrentOrder = order;
        IsOrderFound = true;
        IsOverdue = order.CurrentStatus == OrderStatus.InStorage && order.PlannedIssueDate.Date < DateTime.Now.Date;

        ItemChecks = new ObservableCollection<OrderItemCheckViewModel>(
            order.Items.Select(i => new OrderItemCheckViewModel
            {
                Item = i,
                IsSelected = true
            }));

        StatusMessage = "";
    }

    [RelayCommand]
    private void RequestFullIssue()
    {
        if (CurrentOrder == null) return;
        if (CurrentOrder.CurrentStatus != OrderStatus.InStorage && CurrentOrder.CurrentStatus != OrderStatus.Accepted)
        {
            StatusMessage = "Заказ не может быть выдан";
            return;
        }
        PendingResult = IssueResult.Issued;
        ShowConfirmation = true;
    }

    [RelayCommand]
    private void RequestPartialIssue()
    {
        if (CurrentOrder == null) return;
        PendingResult = IssueResult.Partial;
        ShowConfirmation = true;
    }

    [RelayCommand]
    private void RequestRefuse()
    {
        if (CurrentOrder == null) return;
        if (string.IsNullOrWhiteSpace(SelectedReturnReason))
        {
            StatusMessage = "Выберите причину возврата";
            return;
        }
        PendingResult = IssueResult.Refused;
        ShowConfirmation = true;
    }

    [RelayCommand]
    private async Task ConfirmOperationAsync()
    {
        if (CurrentOrder == null || !PendingResult.HasValue) return;

        try
        {
            switch (PendingResult.Value)
            {
                case IssueResult.Issued:
                case IssueResult.Partial:
                    var issuedItems = PendingResult.Value == IssueResult.Partial
                        ? ItemChecks.Where(x => x.IsSelected).Select(x => x.Item).ToList()
                        : null;
                    var total = CurrentOrder.Items.Sum(i => i.Price * i.Quantity);
                    await _orderService.IssueOrderAsync(CurrentOrder.Id, _currentEmployeeId, _currentShiftId,
                        PendingResult.Value, total, null, issuedItems);
                    StatusMessage = "Заказ выдан!";
                    break;

                case IssueResult.Refused:
                    await _orderService.ReturnOrderAsync(CurrentOrder.Id, _currentEmployeeId, _currentShiftId, SelectedReturnReason);
                    StatusMessage = "Возврат оформлен!";
                    break;
            }

            ShowConfirmation = false;
            await Task.Delay(1500);
            OperationCompleted?.Invoke(true);
            ClearForm();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    [RelayCommand]
    private void CancelConfirmation()
    {
        ShowConfirmation = false;
        PendingResult = null;
    }

    [RelayCommand]
    private void ClearForm()
    {
        CurrentOrder = null;
        IsOrderFound = false;
        IsOverdue = false;
        Barcode = "";
        StatusMessage = "";
        ShowConfirmation = false;
        PendingResult = null;
        ItemChecks.Clear();
        SelectedReturnReason = "";
    }
}

public partial class OrderItemCheckViewModel : ObservableObject
{
    [ObservableProperty]
    private OrderItem _item = null!;

    [ObservableProperty]
    private bool _isSelected = true;
}