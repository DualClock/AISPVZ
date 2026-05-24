using AISPVZ.Data.Context;
using AISPVZ.Models;
using Microsoft.EntityFrameworkCore;

namespace AISPVZ.Services;

public class ReportService
{
    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
    {
        using var db = new AppDbContext();
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        return await db.IssueOperations
            .Where(io => io.IssueDateTime >= start && io.IssueDateTime < end)
            .SumAsync(io => io.TotalAmount);
    }

    public async Task<int> GetCompletedOrdersCountAsync(int year, int month)
    {
        using var db = new AppDbContext();
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);
        return await db.IssueOperations
            .Where(io => io.IssueDateTime >= start && io.IssueDateTime < end)
            .CountAsync();
    }

    public async Task<List<TopProductItem>> GetTopProductsAsync(int year, int month, int top = 5)
    {
        using var db = new AppDbContext();
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1);

        var items = await db.OrderItems
            .Where(oi => oi.Order.IssueOperations.Any(io => io.IssueDateTime >= start && io.IssueDateTime < end))
            .GroupBy(oi => oi.ProductName)
            .Select(g => new TopProductItem
            {
                ProductName = g.Key,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(top)
            .ToListAsync();

        return items;
    }

    public async Task<DashboardReportData> GetDashboardReportDataAsync()
    {
        using var db = new AppDbContext();
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var startOfPrevMonth = startOfMonth.AddMonths(-1);

        var revenue = await db.IssueOperations
            .Where(io => io.IssueDateTime >= startOfMonth)
            .SumAsync(io => io.TotalAmount);

        var completed = await db.IssueOperations
            .Where(io => io.IssueDateTime >= startOfMonth)
            .CountAsync();

        var returns = await db.ReturnOperations
            .Where(ro => ro.ReturnDateTime >= startOfMonth)
            .CountAsync();

        var topProducts = await db.OrderItems
            .Where(oi => oi.Order.IssueOperations.Any(io => io.IssueDateTime >= startOfMonth))
            .GroupBy(oi => oi.ProductName)
            .Select(g => new TopProductItem
            {
                ProductName = g.Key,
                TotalQuantity = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(5)
            .ToListAsync();

        return new DashboardReportData
        {
            CurrentMonthRevenue = revenue,
            CurrentMonthCompleted = completed,
            CurrentMonthReturns = returns,
            TopProducts = topProducts
        };
    }
}

public class TopProductItem
{
    public string ProductName { get; set; } = "";
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class DashboardReportData
{
    public decimal CurrentMonthRevenue { get; set; }
    public int CurrentMonthCompleted { get; set; }
    public int CurrentMonthReturns { get; set; }
    public List<TopProductItem> TopProducts { get; set; } = new();
}
