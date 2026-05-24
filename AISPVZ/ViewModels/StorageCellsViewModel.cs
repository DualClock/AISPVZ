using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using System.Collections.ObjectModel;

namespace AISPVZ.ViewModels;

public partial class StorageCellsViewModel : ObservableObject
{
    private readonly ReferenceService _referenceService;

    [ObservableProperty]
    private ObservableCollection<StorageCell> _cells = new();

    [ObservableProperty]
    private StorageCell? _selectedCell;

    [ObservableProperty]
    private string _filterZone = "";

    [ObservableProperty]
    private bool _filterBusyOnly;

    [ObservableProperty]
    private string _newCellCode = "";

    [ObservableProperty]
    private string _newCellZone = "A";

    [ObservableProperty]
    private double _newCellMaxWeight = 30;

    [ObservableProperty]
    private string _newCellComment = "";

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _statusMessage = "";

    public event Action? DataChanged;

    public StorageCellsViewModel()
    {
        _referenceService = new ReferenceService();
    }

    [RelayCommand]
    private async Task LoadCellsAsync()
    {
        var cells = await _referenceService.GetAllCellsAsync();
        ApplyFilter(cells);
    }

    private void ApplyFilter(List<StorageCell> allCells)
    {
        var filtered = allCells.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(FilterZone))
            filtered = filtered.Where(c => c.Zone.Equals(FilterZone, StringComparison.OrdinalIgnoreCase));

        if (FilterBusyOnly)
            filtered = filtered.Where(c => c.IsBusy);

        Cells = new ObservableCollection<StorageCell>(filtered.OrderBy(c => c.Zone).ThenBy(c => c.CellCode));
    }

    [RelayCommand]
    private async Task ApplyFilterCommand()
    {
        var cells = await _referenceService.GetAllCellsAsync();
        ApplyFilter(cells);
    }

    [RelayCommand]
    private void StartAdd()
    {
        SelectedCell = null;
        NewCellCode = "";
        NewCellZone = "A";
        NewCellMaxWeight = 30;
        NewCellComment = "";
        IsEditing = true;
    }

    [RelayCommand]
    private void StartEdit()
    {
        if (SelectedCell == null) return;
        NewCellCode = SelectedCell.CellCode;
        NewCellZone = SelectedCell.Zone;
        NewCellMaxWeight = SelectedCell.MaxWeightKg;
        NewCellComment = SelectedCell.Comment ?? "";
        IsEditing = true;
    }

    [RelayCommand]
    private async Task SaveCellAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCellCode))
        {
            StatusMessage = "Введите код ячейки";
            return;
        }

        try
        {
            if (SelectedCell != null)
            {
                SelectedCell.CellCode = NewCellCode;
                SelectedCell.Zone = NewCellZone;
                SelectedCell.MaxWeightKg = NewCellMaxWeight;
                SelectedCell.Comment = NewCellComment;
                await _referenceService.UpdateCellAsync(SelectedCell);
                StatusMessage = "Ячейка обновлена";
            }
            else
            {
                var cell = new StorageCell
                {
                    CellCode = NewCellCode,
                    Zone = NewCellZone,
                    MaxWeightKg = NewCellMaxWeight,
                    Comment = NewCellComment
                };
                await _referenceService.CreateCellAsync(cell);
                StatusMessage = "Ячейка добавлена";
            }

            IsEditing = false;
            await LoadCellsAsync();
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