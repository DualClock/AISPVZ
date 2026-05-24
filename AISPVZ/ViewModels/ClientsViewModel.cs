using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;

namespace AISPVZ.ViewModels;

public partial class ClientsViewModel : ObservableObject
{
    private readonly ReferenceService _referenceService;
    private readonly OrderService _orderService;

    [ObservableProperty]
    private ObservableCollection<Client> _clients = new();

    [ObservableProperty]
    private Client? _selectedClient;

    [ObservableProperty]
    private ObservableCollection<Order> _clientOrders = new();

    [ObservableProperty]
    private string _searchQuery = "";

    [ObservableProperty]
    private string _fullName = "";

    [ObservableProperty]
    private string _phone = "";

    [ObservableProperty]
    private string _email = "";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _showOrders;

    [ObservableProperty]
    private string _statusMessage = "";

    public event Action? DataChanged;

    public ClientsViewModel()
    {
        _referenceService = new ReferenceService();
        _orderService = new OrderService();
    }

    [RelayCommand]
    private async Task LoadClientsAsync()
    {
        var clist = string.IsNullOrWhiteSpace(SearchQuery)
            ? await _referenceService.GetAllClientsAsync()
            : await _referenceService.SearchClientsAsync(SearchQuery);
        Clients = new ObservableCollection<Client>(clist);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadClientsAsync();
    }

    [RelayCommand]
    private void StartAdd()
    {
        SelectedClient = null;
        FullName = "";
        Phone = "";
        Email = "";
        IsEditing = true;
    }

    [RelayCommand]
    private void StartEdit()
    {
        if (SelectedClient == null) return;
        FullName = SelectedClient.FullName;
        Phone = SelectedClient.Phone;
        Email = SelectedClient.Email ?? "";
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveClientAsync()
    {
        if (string.IsNullOrWhiteSpace(FullName))
        {
            StatusMessage = "Введите ФИО";
            return;
        }

        try
        {
            if (SelectedClient != null)
            {
                SelectedClient.FullName = FullName;
                SelectedClient.Phone = Phone;
                SelectedClient.Email = Email;
                await _referenceService.UpdateClientAsync(SelectedClient);
                StatusMessage = "Клиент обновлён";
            }
            else
            {
                var client = new Client
                {
                    FullName = FullName,
                    Phone = Phone,
                    Email = Email
                };
                await _referenceService.CreateClientAsync(client);
                StatusMessage = "Клиент добавлен";
            }

            IsEditing = false;
            await LoadClientsAsync();
            DataChanged?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task ShowClientOrdersAsync()
    {
        if (SelectedClient == null) return;
        var orders = await _orderService.SearchByPhoneAsync(SelectedClient.Phone);
        ClientOrders = new ObservableCollection<Order>(orders);
        ShowOrders = true;
    }

    [RelayCommand]
    private void HideOrders()
    {
        ShowOrders = false;
        ClientOrders.Clear();
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
    }
}