using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;
using AISPVZ.Data.Context;

namespace AISPVZ.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly ExportService _exportService;

    [ObservableProperty]
    private DateTime _reportStartDate = DateTime.Now.AddDays(-7);

    [ObservableProperty]
    private DateTime _reportEndDate = DateTime.Now;

    [ObservableProperty]
    private ObservableCollection<ShiftReportItem> _shiftReportItems = new();

    [ObservableProperty]
    private ObservableCollection<OverdueReportItem> _overdueItems = new();

    [ObservableProperty]
    private ObservableCollection<MarketplaceStatsItem> _marketplaceStats = new();

    [ObservableProperty]
    private Shift? _selectedShift;

    [ObservableProperty]
    private ObservableCollection<Shift> _closedShifts = new();

    [ObservableProperty]
    private string _statusMessage = "";

    private int _currentEmployeeId;
    private int _currentShiftId;

    public ReportsViewModel()
    {
        _exportService = new ExportService();
    }

    public void SetContext(int employeeId, int shiftId)
    {
        _currentEmployeeId = employeeId;
        _currentShiftId = shiftId;
    }

    [RelayCommand]
    private async Task LoadClosedShiftsAsync()
    {
        using var db = new AppDbContext();
        var shifts = await db.Shifts
            .Include(s => s.Employee)
            .Where(s => s.IsClosed)
            .OrderByDescending(s => s.StartTime)
            .Take(30)
            .ToListAsync();
        ClosedShifts = new ObservableCollection<Shift>(shifts);
    }

    [RelayCommand]
    private async Task GenerateShiftReportAsync()
    {
        if (SelectedShift == null) return;

        using var db = new AppDbContext();
        var issues = await db.IssueOperations
            .Include(i => i.Order).ThenInclude(o => o.Client)
            .Include(i => i.Employee)
            .Where(i => i.ShiftId == SelectedShift.Id)
            .ToListAsync();

        var returns = await db.ReturnOperations
            .Include(r => r.Order).ThenInclude(o => o.Client)
            .Include(r => r.Employee)
            .Where(r => r.ShiftId == SelectedShift.Id)
            .ToListAsync();

        ShiftReportItems = new ObservableCollection<ShiftReportItem>(
            issues.Select(i => new ShiftReportItem
            {
                Time = i.IssueDateTime,
                OrderBarcode = i.Order.Barcode,
                ClientName = i.Order.Client.FullName,
                Result = i.Result.ToString(),
                Amount = i.TotalAmount,
                EmployeeName = i.Employee.FullName
            }));

        foreach (var r in returns)
        {
            ShiftReportItems.Add(new ShiftReportItem
            {
                Time = r.ReturnDateTime,
                OrderBarcode = r.Order.Barcode,
                ClientName = r.Order.Client.FullName,
                Result = "Возврат",
                Amount = 0,
                EmployeeName = r.Employee.FullName
            });
        }

        StatusMessage = $"Отчёт по смене: {issues.Count} выдач, {returns.Count} возвратов";
    }

    [RelayCommand]
    private async Task GenerateOverdueReportAsync()
    {
        using var db = new AppDbContext();
        var today = DateTime.Now.Date;
        var overdue = await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Where(o => o.CurrentStatus == OrderStatus.InStorage && o.PlannedIssueDate < today)
            .OrderBy(o => o.PlannedIssueDate)
            .ToListAsync();

        OverdueItems = new ObservableCollection<OverdueReportItem>(
            overdue.Select(o => new OverdueReportItem
            {
                Barcode = o.Barcode,
                ClientName = o.Client.FullName,
                ClientPhone = o.Client.Phone,
                CellCode = o.Cell?.CellCode ?? "-",
                PlannedDate = o.PlannedIssueDate,
                DaysOverdue = (today - o.PlannedIssueDate.Date).Days
            }));
    }

    [RelayCommand]
    private async Task GenerateMarketplaceReportAsync()
    {
        using var db = new AppDbContext();
        var stats = await db.Orders
            .Where(o => o.ArrivedAt >= ReportStartDate && o.ArrivedAt <= ReportEndDate)
            .GroupBy(o => o.Marketplace)
            .Select(g => new { Marketplace = g.Key, Count = g.Count(), TotalAmount = g.Sum(o => o.Items.Sum(i => i.Price * i.Quantity)) })
            .ToListAsync();

        MarketplaceStats = new ObservableCollection<MarketplaceStatsItem>(
            stats.Select(s => new MarketplaceStatsItem
            {
                MarketplaceName = s.Marketplace.ToString(),
                OrderCount = s.Count,
                TotalAmount = s.TotalAmount
            }));
    }

    [RelayCommand]
    private async Task ExportShiftReportToCsvAsync()
    {
        if (ShiftReportItems.Count == 0)
        {
            StatusMessage = "Нет данных для экспорта";
            return;
        }

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"Сменный_отчёт_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        await _exportService.ExportToCsvAsync(ShiftReportItems, path);
        StatusMessage = $"Экспортировано: {path}";
    }

    [RelayCommand]
    private async Task ExportOverdueReportToCsvAsync()
    {
        if (OverdueItems.Count == 0)
        {
            StatusMessage = "Нет данных для экспорта";
            return;
        }

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"Просроченные_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        await _exportService.ExportToCsvAsync(OverdueItems, path);
        StatusMessage = $"Экспортировано: {path}";
    }

    [RelayCommand]
    private async Task ExportMarketplaceReportToCsvAsync()
    {
        if (MarketplaceStats.Count == 0)
        {
            StatusMessage = "Нет данных для экспорта";
            return;
        }

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"Маркетплейсы_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        await _exportService.ExportToCsvAsync(MarketplaceStats, path);
        StatusMessage = $"Экспортировано: {path}";
    }
}

public class ShiftReportItem
{
    public DateTime Time { get; set; }
    public string OrderBarcode { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string Result { get; set; } = "";
    public decimal Amount { get; set; }
    public string EmployeeName { get; set; } = "";
}

public class OverdueReportItem
{
    public string Barcode { get; set; } = "";
    public string ClientName { get; set; } = "";
    public string ClientPhone { get; set; } = "";
    public string CellCode { get; set; } = "";
    public DateTime PlannedDate { get; set; }
    public int DaysOverdue { get; set; }
}

public class MarketplaceStatsItem
{
    public string MarketplaceName { get; set; } = "";
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
}