using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AISPVZ.Models;
using AISPVZ.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.IO;

namespace AISPVZ.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    private readonly ReportService _reportService;
    private readonly ExportService _exportService;

    // Dashboard metrics
    [ObservableProperty]
    private decimal _currentMonthRevenue;

    [ObservableProperty]
    private int _currentMonthCompleted;

    [ObservableProperty]
    private int _currentMonthReturns;

    [ObservableProperty]
    private ObservableCollection<TopProductItem> _topProducts = new();

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private DateTime _reportMonth = DateTime.Now;

    // Legacy shift report fields (kept for compatibility)
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

    private int _currentEmployeeId;
    private int _currentShiftId;

    public ReportsViewModel()
    {
        _reportService = new ReportService();
        _exportService = new ExportService();
    }

    public void SetContext(int employeeId, int shiftId)
    {
        _currentEmployeeId = employeeId;
        _currentShiftId = shiftId;
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        try
        {
            StatusMessage = "Загрузка...";
            var data = await _reportService.GetDashboardReportDataAsync();
            CurrentMonthRevenue = data.CurrentMonthRevenue;
            CurrentMonthCompleted = data.CurrentMonthCompleted;
            CurrentMonthReturns = data.CurrentMonthReturns;
            TopProducts = new ObservableCollection<TopProductItem>(data.TopProducts);
            StatusMessage = $"Данные за {DateTime.Now:MMMM yyyy} загружены";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task LoadMonthReportAsync()
    {
        try
        {
            StatusMessage = "Загрузка...";
            var revenue = await _reportService.GetMonthlyRevenueAsync(ReportMonth.Year, ReportMonth.Month);
            var completed = await _reportService.GetCompletedOrdersCountAsync(ReportMonth.Year, ReportMonth.Month);
            var top = await _reportService.GetTopProductsAsync(ReportMonth.Year, ReportMonth.Month, 5);

            CurrentMonthRevenue = revenue;
            CurrentMonthCompleted = completed;
            TopProducts = new ObservableCollection<TopProductItem>(top);
            StatusMessage = $"Отчёт за {ReportMonth:MMMM yyyy} загружен";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка загрузки: " + ex.Message;
        }
    }

    [RelayCommand]
    private async Task ExportTopProductsToCsvAsync()
    {
        if (TopProducts.Count == 0)
        {
            StatusMessage = "Нет данных для экспорта";
            return;
        }

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"ТопТоваров_{ReportMonth:yyyyMM}.csv");
        await _exportService.ExportToCsvAsync(TopProducts, path);
        StatusMessage = $"Экспортировано: {path}";
    }

    // Legacy commands preserved for compatibility
    [RelayCommand]
    private async Task LoadClosedShiftsAsync()
    {
        using var db = new AISPVZ.Data.Context.AppDbContext();
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
        using var db = new AISPVZ.Data.Context.AppDbContext();
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
        using var db = new AISPVZ.Data.Context.AppDbContext();
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
        using var db = new AISPVZ.Data.Context.AppDbContext();
        var stats = await db.Orders
            .Where(o => o.ArrivedAt >= ReportMonth.AddMonths(-1) && o.ArrivedAt <= ReportMonth)
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
