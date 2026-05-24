using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AISPVZ.ViewModels;

public partial class IssueOrderViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly ShiftService _shiftService;
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private Order? _selectedOrder;

    [ObservableProperty]
    private ObservableCollection<Order> _activeOrders = new();

    [ObservableProperty]
    private ObservableCollection<OrderItemDisplay> _orderItems = new();

    [ObservableProperty]
    private OrderItemDisplay? _selectedOrderItem;

    [ObservableProperty]
    private string _barcode = "";

    [ObservableProperty]
    private bool _isOrderSelected;

    [ObservableProperty]
    private bool _isOverdue;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _showConfirmation;

    [ObservableProperty]
    private string _confirmationMessage = "";

    [ObservableProperty]
    private string _selectedReturnReason = "";

    [ObservableProperty]
    private ObservableCollection<string> _returnReasons = new();

    [ObservableProperty]
    private bool _isReturnMode;

    [ObservableProperty]
    private string _windowTitle = "ВЫДАЧА ЗАКАЗА";

    [ObservableProperty]
    private string _windowSubtitle = "Выберите заказ из списка слева";

    [ObservableProperty]
    private string _itemsSummary = "";

    private int _currentShiftId;
    private int _currentEmployeeId;
    private Employee? _currentEmployee;
    private bool _isAdmin;
    private IssueOperationType _pendingOperation;

    public event Action<bool>? OperationCompleted;

    public IssueOrderViewModel()
    {
        _orderService = new OrderService();
        _shiftService = new ShiftService();
        _referenceService = new ReferenceService();
    }

    public async Task InitializeAsync(int employeeId, int shiftId, bool isAdmin = false, bool isReturnMode = false, Employee? currentEmployee = null)
    {
        _currentEmployeeId = employeeId;
        _currentShiftId = shiftId;
        _isAdmin = isAdmin;
        _currentEmployee = currentEmployee;
        IsReturnMode = isReturnMode;

        if (isReturnMode)
        {
            WindowTitle = "ВОЗВРАТ ЗАКАЗА";
            WindowSubtitle = "Выберите заказ и укажите причину возврата";
        }

        try
        {
            var reasons = await _referenceService.GetReturnReasonsAsync();
            ReturnReasons = new ObservableCollection<string>(reasons);
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки причин: " + ex.Message;
            HasError = true;
        }

        await LoadActiveOrdersAsync();
    }

    private async Task LoadActiveOrdersAsync()
    {
        try
        {
            HasError = false;
            StatusMessage = "";
            var orders = await _orderService.GetActiveOrdersAsync();
            ActiveOrders = new ObservableCollection<Order>(orders);
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки заказов: " + ex.Message;
            HasError = true;
        }
    }

    partial void OnSelectedOrderChanged(Order? value)
    {
        OrderItems.Clear();
        SelectedOrderItem = null;
        IsOrderSelected = false;
        IsOverdue = false;
        ItemsSummary = "";

        if (value == null)
        {
            StatusMessage = "";
            HasError = false;
            return;
        }

        try
        {
            IsOrderSelected = true;
            IsOverdue = value.CurrentStatus == OrderStatus.InStorage && value.PlannedIssueDate.Date < DateTime.Now.Date;

            foreach (var item in value.Items)
            {
                OrderItems.Add(new OrderItemDisplay
                {
                    Id = item.Id,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    IsIssued = item.IsIssued,
                    IsSelected = !item.IsIssued
                });
            }

            var totalQty = value.Items.Sum(i => i.Quantity);
            var totalSum = value.Items.Sum(i => i.Price * i.Quantity);
            ItemsSummary = $"Позиций: {value.Items.Count} | Кол-во: {totalQty} шт. | Сумма: {totalSum:N2} ₽";

            StatusMessage = "";
            HasError = false;
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки товаров: " + ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private async Task SearchByBarcodeAsync()
    {
        if (string.IsNullOrWhiteSpace(Barcode))
        {
            StatusMessage = "Введите штрихкод";
            HasError = true;
            return;
        }

        try
        {
            HasError = false;
            var order = await _orderService.GetByBarcodeAsync(Barcode);
            if (order == null)
            {
                StatusMessage = "Заказ не найден";
                HasError = true;
                return;
            }

            SelectedOrder = order;
            Barcode = "";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка поиска: " + ex.Message;
            HasError = true;
        }
    }

    [RelayCommand]
    private void IssueFull()
    {
        if (!ValidateBeforeIssue()) return;

        _pendingOperation = IssueOperationType.FullIssue;
        ConfirmationMessage = $"Выдать заказ {SelectedOrder!.Barcode} клиенту {SelectedOrder.Client?.FullName}?\nВсего позиций: {OrderItems.Count}";
        ShowConfirmation = true;
    }

    [RelayCommand]
    private void IssueSelected()
    {
        if (!ValidateBeforeIssue()) return;

        var selected = OrderItems.Where(i => i.IsSelected && !i.IsIssued).ToList();
        if (selected.Count == 0)
        {
            StatusMessage = "Выберите хотя бы один не выданный товар";
            HasError = true;
            return;
        }

        _pendingOperation = IssueOperationType.PartialIssue;
        var total = selected.Sum(i => i.Price * i.Quantity);
        ConfirmationMessage = $"Частичная выдача: {selected.Count} поз. на сумму {total:N2} ₽";
        ShowConfirmation = true;
    }

    [RelayCommand]
    private void ProcessReturn()
    {
        if (SelectedOrder == null)
        {
            StatusMessage = "Сначала выберите заказ";
            HasError = true;
            return;
        }

        if (!ValidateShift()) return;

        if (string.IsNullOrWhiteSpace(SelectedReturnReason))
        {
            StatusMessage = "Выберите причину возврата";
            HasError = true;
            return;
        }

        _pendingOperation = IssueOperationType.Return;
        ConfirmationMessage = $"Оформить возврат заказа {SelectedOrder.Barcode}?\nПричина: {SelectedReturnReason}";
        ShowConfirmation = true;
    }

    [RelayCommand]
    private async Task ConfirmOperationAsync()
    {
        if (SelectedOrder == null || _currentShiftId == 0)
        {
            StatusMessage = "Ошибка: заказ не выбран или смена не открыта";
            HasError = true;
            ShowConfirmation = false;
            return;
        }

        try
        {
            switch (_pendingOperation)
            {
                case IssueOperationType.FullIssue:
                    {
                        var total = OrderItems.Sum(i => i.Price * i.Quantity);
                        await _orderService.IssueOrderAsync(
                            SelectedOrder.Id, _currentEmployeeId, _currentShiftId,
                            IssueResult.Issued, total, null, null);
                        StatusMessage = "✓ Заказ успешно выдан!";
                        HasError = false;
                    }
                    break;

                case IssueOperationType.PartialIssue:
                    {
                        var selected = OrderItems.Where(i => i.IsSelected && !i.IsIssued).ToList();
                        if (selected.Count == 0)
                        {
                            StatusMessage = "Ошибка: выберите товары для выдачи";
                            HasError = true;
                            ShowConfirmation = false;
                            return;
                        }

                        var total = selected.Sum(i => i.Price * i.Quantity);
                        var itemsToIssue = selected.Select(i => new OrderItem
                        {
                            Id = i.Id,
                            ProductName = i.ProductName,
                            Quantity = i.Quantity,
                            Price = i.Price
                        }).ToList();

                        await _orderService.IssueOrderAsync(
                            SelectedOrder.Id, _currentEmployeeId, _currentShiftId,
                            IssueResult.Partial, total, null, itemsToIssue);
                        StatusMessage = "✓ Частичная выдача оформлена!";
                        HasError = false;
                    }
                    break;

                case IssueOperationType.Return:
                    await _orderService.ReturnOrderAsync(
                        SelectedOrder.Id, _currentEmployeeId, _currentShiftId, SelectedReturnReason);
                    StatusMessage = "✓ Возврат оформлен!";
                    HasError = false;
                    break;
            }

            ShowConfirmation = false;
            OperationCompleted?.Invoke(true);
            await Task.Delay(1200);
            await RefreshAfterOperationAsync();
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null)
                msg += " | Inner: " + ex.InnerException.Message;
            StatusMessage = "Ошибка операции: " + msg;
            HasError = true;
            ShowConfirmation = false;
        }
    }

    [RelayCommand]
    private void CancelConfirmation()
    {
        ShowConfirmation = false;
    }

    [RelayCommand]
    private async Task PrintReceiptAsync()
    {
        if (SelectedOrder == null) return;
        try
        {
            if (_currentEmployee == null)
            {
                var refService = new ReferenceService();
                var employees = await refService.GetAllEmployeesAsync();
                _currentEmployee = employees.FirstOrDefault(e => e.Id == _currentEmployeeId);
            }

            var printService = new PrintService();
            var total = OrderItems.Sum(i => i.Price * i.Quantity);
            if (_currentEmployee != null)
            {
                printService.PrintIssueReceipt(SelectedOrder, _currentEmployee, total);
                StatusMessage = "Чек отправлен на печать";
                HasError = false;
            }
            else
            {
                StatusMessage = "Ошибка: сотрудник не найден";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка печати: " + ex.Message;
            HasError = true;
        }
    }

    private bool ValidateBeforeIssue()
    {
        if (SelectedOrder == null)
        {
            StatusMessage = "Сначала выберите заказ";
            HasError = true;
            return false;
        }
        if (SelectedOrder.CurrentStatus != OrderStatus.InStorage &&
            SelectedOrder.CurrentStatus != OrderStatus.Accepted &&
            SelectedOrder.CurrentStatus != OrderStatus.PartialIssued)
        {
            StatusMessage = "Заказ не может быть выдан (статус: " + SelectedOrder.CurrentStatus + ")";
            HasError = true;
            return false;
        }
        return ValidateShift();
    }

    private bool ValidateShift()
    {
        if (_currentShiftId == 0)
        {
            StatusMessage = "Ошибка: Смена не открыта!";
            HasError = true;
            return false;
        }
        return true;
    }

    private async Task RefreshAfterOperationAsync()
    {
        SelectedOrder = null;
        OrderItems.Clear();
        SelectedOrderItem = null;
        IsOrderSelected = false;
        IsOverdue = false;
        Barcode = "";
        StatusMessage = "";
        HasError = false;
        ShowConfirmation = false;
        ItemsSummary = "";
        await LoadActiveOrdersAsync();
    }
}

public enum IssueOperationType
{
    FullIssue,
    PartialIssue,
    Return
}

public class OrderItemDisplay : INotifyPropertyChanged
{
    private bool _isSelected = true;
    private bool _isIssued;
    private int _quantity;
    private decimal _price;
    private string _productName = "";

    public int Id { get; set; }

    public string ProductName
    {
        get => _productName;
        set { _productName = value; OnPropertyChanged(nameof(ProductName)); }
    }

    public int Quantity
    {
        get => _quantity;
        set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(TotalPrice)); }
    }

    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(nameof(Price)); OnPropertyChanged(nameof(TotalPrice)); }
    }

    public bool IsIssued
    {
        get => _isIssued;
        set { _isIssued = value; OnPropertyChanged(nameof(IsIssued)); OnPropertyChanged(nameof(StatusText)); }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    public decimal TotalPrice => Price * Quantity;

    public string StatusText => IsIssued ? "Выдан" : "На складе";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
