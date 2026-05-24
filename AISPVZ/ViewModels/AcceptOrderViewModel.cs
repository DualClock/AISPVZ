using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;

namespace AISPVZ.ViewModels;

public partial class AcceptOrderViewModel : ObservableObject
{
    private readonly OrderService _orderService;
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private string _barcode = "";

    [ObservableProperty]
    private string _marketplaceName = "";

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
    private string _comment = "";

    [ObservableProperty]
    private int _plannedStorageDays = 7;

    [ObservableProperty]
    private ObservableCollection<OrderItemViewModel> _items = new();

    [ObservableProperty]
    private ObservableCollection<StorageCell> _freeCells = new();

    [ObservableProperty]
    private bool _isAddingItem;

    [ObservableProperty]
    private string _itemsCountText = "Нет товаров";

    [ObservableProperty]
    private string _totalAmountText = "Итого: 0 ₽";

    // New item form fields
    [ObservableProperty]
    private string _newItemArticle = "";

    [ObservableProperty]
    private string _newItemName = "";

    [ObservableProperty]
    private string _newItemQuantity = "1";

    [ObservableProperty]
    private string _newItemPrice = "";

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action<bool>? OperationCompleted;

    public AcceptOrderViewModel()
    {
        _orderService = new OrderService();
        _referenceService = new ReferenceService();
    }

    public async Task LoadDataAsync()
    {
        var cells = await _referenceService.GetFreeCellsAsync();
        FreeCells = new ObservableCollection<StorageCell>(cells);
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

        ItemsCountText = Items.Count == 0 ? "Нет товаров" : $"{Items.Count} поз. • {totalQty} шт.";
        TotalAmountText = $"Итого: {totalSum:N2} ₽";
    }

    [RelayCommand]
    private async Task SearchClientByPhoneAsync()
    {
        if (string.IsNullOrWhiteSpace(ClientPhone)) return;

        IsLoading = true;
        StatusMessage = "Поиск...";
        try
        {
            var client = await _orderService.FindClientByPhoneAsync(ClientPhone);
            if (client != null)
            {
                SelectedClient = client;
                ClientName = client.FullName;
                ClientEmail = client.Email ?? "";
                StatusMessage = $"Найден: {client.FullName}";
            }
            else
            {
                SelectedClient = null;
                StatusMessage = "Не найден. Буд��т создан новый.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
        finally
        {
            IsLoading = false;
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
            StatusMessage = "Введите наименование товара";
            return;
        }

        int qty = 1;
        if (!int.TryParse(NewItemQuantity, out qty) || qty < 1)
        {
            StatusMessage = "Введите корректное количество";
            return;
        }

        decimal price = 0;
        if (!string.IsNullOrWhiteSpace(NewItemPrice))
        {
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
        StatusMessage = "";
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
        await SaveOrderInternalAsync(print: false);
    }

    [RelayCommand]
    private async Task SaveAndPrintOrderAsync()
    {
        await SaveOrderInternalAsync(print: true);
    }

    private async Task SaveOrderInternalAsync(bool print)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(Barcode))
        {
            StatusMessage = "Введите штрихкод заказа";
            return;
        }

        if (string.IsNullOrWhiteSpace(ClientName))
        {
            StatusMessage = "Введите ФИО клиента";
            return;
        }

        if (!Items.Any())
        {
            StatusMessage = "Добавьте хотя бы один товар";
            return;
        }

        IsLoading = true;
        StatusMessage = "Сохранение...";

        try
        {
            // Prepare items - ensure ProductName is never empty
            var itemsList = Items
                .Where(i => !string.IsNullOrWhiteSpace(i.ProductName))
                .Select(i => new OrderItem
                {
                    ProductName = string.IsNullOrWhiteSpace(i.ProductName) ? "Товар" : i.ProductName,
                    Article = i.Article ?? "",
                    Quantity = Math.Max(1, i.Quantity),
                    Price = i.Price
                }).ToList();

            if (itemsList.Count == 0)
            {
                StatusMessage = "Добавьте хотя бы один товар";
                IsLoading = false;
                return;
            }

            // Get max storage days from settings
            var maxDaysSetting = await _referenceService.GetSettingAsync("MaxStorageDays");
            int maxDays = int.TryParse(maxDaysSetting, out var md) ? md : 7;

            // Call service with simple parameters - NO entity navigation properties
            var resultBarcode = await _orderService.CreateOrderSimpleAsync(
                clientName: ClientName,
                clientPhone: ClientPhone,
                clientEmail: ClientEmail,
                cellId: SelectedCell?.Id,
                barcode: Barcode,
                marketplace: ParseMarketplace(MarketplaceName),
                comment: Comment,
                items: itemsList,
                maxStorageDays: maxDays
            );

            StatusMessage = "Заказ принят! Штрихкод: " + resultBarcode;

            if (print)
            {
                // Load full order for printing
                var order = await _orderService.GetByBarcodeAsync(resultBarcode);
                if (order != null)
                {
                    var printService = new PrintService();
                    printService.PrintAcceptReceipt(order);
                }
            }

            OperationCompleted?.Invoke(true);
            await Task.Delay(1500);
            ClearForm();
        }
        catch (Exception ex)
        {
            var msg = ex.Message;
            if (ex.InnerException != null)
                msg += " | Inner: " + ex.InnerException.Message;
            if (ex is Microsoft.EntityFrameworkCore.DbUpdateException dbEx && dbEx.Entries != null)
            {
                foreach (var entry in dbEx.Entries)
                {
                    msg += $" | Entity: {entry.Entity.GetType().Name}, State: {entry.State}";
                }
            }
            StatusMessage = "Ошибка: " + msg;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void ClearForm()
    {
        Barcode = "";
        MarketplaceName = "";
        SelectedClient = null;
        ClientName = "";
        ClientPhone = "";
        ClientEmail = "";
        SelectedCell = null;
        Comment = "";
        Items.Clear();
        IsAddingItem = false;
        ClearNewItemFields();
        UpdateSummary();
        StatusMessage = "";
    }

    private static Models.Marketplace ParseMarketplace(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return Models.Marketplace.Other;
        var lower = name.Trim().ToLower();
        if (lower.Contains("ozon")) return Models.Marketplace.Ozon;
        if (lower.Contains("wildber") || lower.Contains("wb")) return Models.Marketplace.Wildberries;
        if (lower.Contains("yandex") || lower.Contains("яндекс")) return Models.Marketplace.YandexMarket;
        return Models.Marketplace.Other;
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
        set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); }
    }

    private decimal _price = 0;
    public decimal Price
    {
        get => _price;
        set { _price = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalPrice)); }
    }

    public decimal TotalPrice => Quantity * Price;
}