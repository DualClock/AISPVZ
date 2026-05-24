using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace AISPVZ.ViewModels;

public partial class AcceptOrderViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private string _barcode = "";

    [ObservableProperty]
    private Marketplace _selectedMarketplace = Marketplace.Ozon;

    [ObservableProperty]
    private string _autoCellZone = "A";

    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private string _clientName = "";

    [ObservableProperty]
    private string _clientPhone = "";

    [ObservableProperty]
    private string _clientEmail = "";

    [ObservableProperty]
    private StorageCell? _selectedCell;

    [ObservableProperty]
    private DateTime _plannedIssueDate = DateTime.Now.AddDays(3);

    [ObservableProperty]
    private string _comment = "";

    [ObservableProperty]
    private ObservableCollection<OrderItemViewModel> _items = new();

    [ObservableProperty]
    private ObservableCollection<StorageCell> _availableCells = new();

    [ObservableProperty]
    private ObservableCollection<StorageCell> _freeCells = new();

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private string _itemsCountText = "Нет товаров";

    [ObservableProperty]
    private string _totalAmountText = "Итого: 0 ₽";

    // New item form fields
    [ObservableProperty]
    private bool _isAddingItem;

    [ObservableProperty]
    private string _newItemArticle = "";

    [ObservableProperty]
    private string _newItemName = "";

    [ObservableProperty]
    private string _newItemQuantity = "1";

    [ObservableProperty]
    private string _newItemPrice = "";

    [ObservableProperty]
    private string _addItemButtonText = "+ Добавить товар";

    // Toast event - raised instead of setting StatusMessage
    public event Action<string, string, bool>? ShowToastNotification;

    public event Action<bool>? OperationCompleted;

    public AcceptOrderViewModel()
    {
        _orderService = new OrderService();
        _referenceService = new ReferenceService();
    }

    private void UpdateSummary()
    {
        int totalQty = 0;
        decimal totalSum = 0;

        foreach (var item in Items)
        {
            if (!string.IsNullOrWhiteSpace(item.ProductName))
            {
                totalQty += item.Quantity;
                totalSum += item.TotalPrice;
            }
        }

        if (Items.Count == 0)
        {
            ItemsCountText = "Нет товаров";
        }
        else
        {
            ItemsCountText = $"{Items.Count} поз. • {totalQty} шт.";
        }
        TotalAmountText = $"Итого: {totalSum:N2} ₽";
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var cells = await _referenceService.GetAllCellsAsync();
        AvailableCells = new ObservableCollection<StorageCell>(cells);

        var freeCells = await _referenceService.GetFreeCellsAsync();
        FreeCells = new ObservableCollection<StorageCell>(freeCells);
    }

    [RelayCommand]
    private async Task SearchClientByPhoneAsync()
    {
        if (string.IsNullOrWhiteSpace(ClientPhone)) return;

        IsSearching = true;
        try
        {
            var client = await _orderService.FindClientByPhoneAsync(ClientPhone);
            if (client != null)
            {
                SelectedClient = client;
                ClientName = client.FullName;
                ClientEmail = client.Email ?? "";
                ShowToastNotification?.Invoke("👤 Клиент найден", $"Загружены данные для {client.FullName}", true);
            }
            else
            {
                ShowToastNotification?.Invoke("⚠ Не найден", "Клиент с таким телефоном не найден", false);
            }
        }
        catch (Exception ex)
        {
            ShowToastNotification?.Invoke("Ошибка", ex.Message, false);
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task AssignAutoCellAsync()
    {
        var cells = await _referenceService.GetFreeCellsAsync(AutoCellZone);
        var firstFree = cells.FirstOrDefault();
        if (firstFree != null)
        {
            SelectedCell = firstFree;
            ShowToastNotification?.Invoke("📦 Ячейка назначена", $"Ячейка {firstFree.CellCode} в зоне {firstFree.Zone}", true);
        }
        else
        {
            ShowToastNotification?.Invoke("⚠ Нет свободных", $"Нет свободных ячеек в зоне {AutoCellZone}", false);
        }
    }

    [RelayCommand]
    private void AddItem()
    {
        IsAddingItem = true;
        ClearNewItemFields();
    }

    [RelayCommand]
    private void ConfirmAddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemName))
        {
            ShowToastNotification?.Invoke("⚠ Внимание", "Введите наименование товара", false);
            return;
        }

        int qty = 1;
        if (!int.TryParse(NewItemQuantity, out qty) || qty < 1)
        {
            ShowToastNotification?.Invoke("⚠ Внимание", "Введите корректное количество", false);
            return;
        }

        decimal price = 0;
        if (!string.IsNullOrWhiteSpace(NewItemPrice))
        {
            // Support both comma and dot decimal separators
            var normalized = NewItemPrice.Replace(',', '.').Trim();
            if (!decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out price))
            {
                price = 0;
            }
        }

        var newItem = new OrderItemViewModel
        {
            Article = NewItemArticle ?? "",
            ProductName = NewItemName,
            Quantity = qty,
            Price = price
        };

        newItem.PropertyChanged += (s, e) => UpdateSummary();
        Items.Add(newItem);
        UpdateSummary();

        IsAddingItem = false;
        ClearNewItemFields();
    }

    [RelayCommand]
    private void CancelAddItem()
    {
        IsAddingItem = false;
        ClearNewItemFields();
    }

    private void ClearNewItemFields()
    {
        NewItemArticle = "";
        NewItemName = "";
        NewItemQuantity = "1";
        NewItemPrice = "";
    }

    [RelayCommand]
    private void RemoveItem(OrderItemViewModel item)
    {
        Items.Remove(item);
        UpdateSummary();
    }

    [RelayCommand]
    private void IncrementQuantity(OrderItemViewModel item)
    {
        item.Quantity++;
        UpdateSummary();
    }

    [RelayCommand]
    private void DecrementQuantity(OrderItemViewModel item)
    {
        if (item.Quantity > 1)
            item.Quantity--;
        UpdateSummary();
    }

    [RelayCommand]
    private async Task SaveOrderAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Barcode))
        {
            ShowToastNotification?.Invoke("⚠ Внимание", "Введите штрихкод заказа", false);
            return;
        }

        if (string.IsNullOrWhiteSpace(ClientName))
        {
            ShowToastNotification?.Invoke("⚠ Внимание", "Введите ФИО клиента", false);
            return;
        }

        if (!Items.Any())
        {
            ShowToastNotification?.Invoke("⚠ Внимание", "Добавьте хотя бы один товар", false);
            return;
        }

        try
        {
            var client = SelectedClient ?? new Client
            {
                FullName = ClientName,
                Phone = ClientPhone,
                Email = ClientEmail
            };

            var order = new Order
            {
                Client = client,
                CellId = SelectedCell?.Id,
                Barcode = Barcode,
                Marketplace = SelectedMarketplace,
                PlannedIssueDate = PlannedIssueDate,
                Comment = Comment,
                CurrentStatus = OrderStatus.InStorage
            };

            var items = Items.Select(i => new OrderItem
            {
                ProductName = i.ProductName,
                Article = i.Article,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            await _orderService.CreateOrderAsync(order, items);

            ShowToastNotification?.Invoke("✓ Успешно", "Заказ принят!", true);

            OperationCompleted?.Invoke(true);

            ClearForm();
        }
        catch (Exception ex)
        {
            ShowToastNotification?.Invoke("❌ Ошибка", "Ошибка сохранения: " + ex.Message, false);
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        Barcode = "";
        SelectedClient = null;
        ClientName = "";
        ClientPhone = "";
        ClientEmail = "";
        SelectedCell = null;
        PlannedIssueDate = DateTime.Now.AddDays(3);
        Comment = "";
        Items.Clear();
        IsAddingItem = false;
        ClearNewItemFields();
        UpdateSummary();
    }

    [RelayCommand]
    private void Cancel()
    {
        Application.Current.MainWindow?.Activate();
    }
}

public partial class OrderItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _article = "";

    [ObservableProperty]
    private string _productName = "";

    private int _quantity = 1;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity != value)
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    private decimal _price = 0;
    public decimal Price
    {
        get => _price;
        set
        {
            if (_price != value)
            {
                _price = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }

    public decimal TotalPrice => Quantity * Price;
}