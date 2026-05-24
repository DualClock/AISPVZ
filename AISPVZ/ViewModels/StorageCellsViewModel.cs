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
    private string _searchText = "";

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

    [ObservableProperty]
    private bool _showVisualScheme;

    [ObservableProperty]
    private ObservableCollection<StorageCell> _schemeCells = new();

    public event Action? DataChanged;
    public event Action<StorageCell>? CellDoubleClicked;

    public StorageCellsViewModel()
    {
        _referenceService = new ReferenceService();
    }

    [RelayCommand]
    private async Task LoadCellsAsync()
    {
        try
        {
            StatusMessage = "";
            var cells = await _referenceService.GetAllCellsAsync();
            ApplyFilter(cells);
            SchemeCells = new ObservableCollection<StorageCell>(cells.OrderBy(c => c.Zone).ThenBy(c => c.CellCode));
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки: " + ex.Message;
        }
    }

    [RelayCommand]
    private void ToggleVisualScheme()
    {
        ShowVisualScheme = !ShowVisualScheme;
    }

    [RelayCommand]
    private void OnCellDoubleClick(StorageCell cell)
    {
        CellDoubleClicked?.Invoke(cell);
    }

    private void ApplyFilter(List<StorageCell> allCells)
    {
        var filtered = allCells.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var q = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(c =>
                c.CellCode.ToLowerInvariant().Contains(q) ||
                (c.Comment?.ToLowerInvariant().Contains(q) ?? false));
        }

        if (!string.IsNullOrWhiteSpace(FilterZone))
            filtered = filtered.Where(c => c.Zone.Equals(FilterZone.Trim(), StringComparison.OrdinalIgnoreCase));

        if (FilterBusyOnly)
            filtered = filtered.Where(c => c.IsBusy);

        Cells = new ObservableCollection<StorageCell>(filtered.OrderBy(c => c.Zone).ThenBy(c => c.CellCode));
    }

    [RelayCommand]
    private async Task DoApplyFilterAsync()
    {
        await LoadCellsAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        _ = DoApplyFilterAsync();
    }

    partial void OnFilterZoneChanged(string value)
    {
        _ = DoApplyFilterAsync();
    }

    partial void OnFilterBusyOnlyChanged(bool value)
    {
        _ = DoApplyFilterAsync();
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
        StatusMessage = "";
    }

    [RelayCommand]
    private void StartEdit()
    {
        if (SelectedCell == null)
        {
            StatusMessage = "Выберите ячейку для редактирования";
            return;
        }
        NewCellCode = SelectedCell.CellCode;
        NewCellZone = SelectedCell.Zone;
        NewCellMaxWeight = SelectedCell.MaxWeightKg;
        NewCellComment = SelectedCell.Comment ?? "";
        IsEditing = true;
        StatusMessage = "";
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
                SelectedCell.CellCode = NewCellCode.Trim();
                SelectedCell.Zone = NewCellZone.Trim();
                SelectedCell.MaxWeightKg = NewCellMaxWeight;
                SelectedCell.Comment = NewCellComment;
                await _referenceService.UpdateCellAsync(SelectedCell);
                StatusMessage = "Ячейка обновлена";
            }
            else
            {
                var cell = new StorageCell
                {
                    CellCode = NewCellCode.Trim(),
                    Zone = NewCellZone.Trim(),
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
            StatusMessage = "Ошибка сохранения: " + ex.Message;
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        StatusMessage = "";
    }
}
