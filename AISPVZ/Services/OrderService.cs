using AISPVZ.Data.Context;
using AISPVZ.Models;
using Microsoft.EntityFrameworkCore;

namespace AISPVZ.Services;

public class OrderService
{
    public async Task<Order?> GetByBarcodeAsync(string barcode)
    {
        using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Barcode == barcode);
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Where(o => o.CurrentStatus == OrderStatus.InStorage || o.CurrentStatus == OrderStatus.Accepted)
            .OrderBy(o => o.PlannedIssueDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        return await GetPendingOrdersAsync();
    }

    public async Task<List<Order>> GetExpiringOrdersAsync(DateTime date)
    {
        using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Where(o => o.CurrentStatus == OrderStatus.InStorage &&
                        o.PlannedIssueDate.Date == date.Date)
            .ToListAsync();
    }

    public async Task<List<Order>> GetOverdueOrdersAsync()
    {
        using var db = new AppDbContext();
        var now = DateTime.Now.Date;
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Where(o => o.CurrentStatus == OrderStatus.InStorage &&
                        o.PlannedIssueDate < now)
            .ToListAsync();
    }

    public async Task<(DashboardStats stats, List<Order> expiringOrders, List<Order> overdueOrders)> GetDashboardDataAsync()
    {
        using var db = new AppDbContext();
        var today = DateTime.Now.Date;
        var now = DateTime.Now;

        var shift = await db.Shifts.FirstOrDefaultAsync(s => !s.IsClosed);
        var issuedToday = 0;
        var returnsToday = 0;
        if (shift != null)
        {
            issuedToday = await db.IssueOperations.CountAsync(io => io.ShiftId == shift.Id && io.IssueDateTime.Date == today);
            returnsToday = await db.ReturnOperations.CountAsync(ro => ro.ShiftId == shift.Id && ro.ReturnDateTime.Date == today);
        }

        var ordersInStorage = await db.Orders.CountAsync(o => o.CurrentStatus == OrderStatus.InStorage);
        var overdue = await db.Orders.CountAsync(o => o.CurrentStatus == OrderStatus.InStorage && o.PlannedIssueDate < today);
        var expiringToday = await db.Orders.CountAsync(o => o.CurrentStatus == OrderStatus.InStorage && o.PlannedIssueDate.Date == today);

        var expiringOrders = await GetExpiringOrdersAsync(today);
        var overdueOrders = await GetOverdueOrdersAsync();

        return (new DashboardStats
        {
            OrdersInStorage = ordersInStorage,
            OverdueOrders = overdue,
            IssuedToday = issuedToday,
            ReturnsToday = returnsToday
        }, expiringOrders, overdueOrders);
    }

    public async Task<Order> CreateOrderAsync(Order order, List<OrderItem> items)
    {
        using var db = new AppDbContext();

        // Find or create client
        var client = await db.Clients.FirstOrDefaultAsync(c => c.Phone == order.Client!.Phone);
        if (client == null)
        {
            db.Clients.Add(order.Client);
            await db.SaveChangesAsync();
            client = order.Client;
        }
        order.ClientId = client.Id;

        // Assign cell if not assigned
        if (order.CellId == null)
        {
            var cell = await db.StorageCells.FirstOrDefaultAsync(c => !c.IsBusy);
            if (cell != null)
            {
                cell.IsBusy = true;
                order.CellId = cell.Id;
            }
        }
        else
        {
            var cell = await db.StorageCells.FindAsync(order.CellId);
            if (cell != null)
                cell.IsBusy = true;
        }

        order.CurrentStatus = OrderStatus.InStorage;
        order.Barcode = GenerateBarcode();

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        // Add items
        foreach (var item in items)
        {
            item.OrderId = order.Id;
            db.OrderItems.Add(item);
        }
        await db.SaveChangesAsync();

        // Status history
        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = OrderStatus.Pending,
            NewStatus = OrderStatus.InStorage,
            ChangedAt = DateTime.Now
        });
        await db.SaveChangesAsync();

        return order;
    }

    public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus, int? employeeId = null)
    {
        using var db = new AppDbContext();
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) return;

        var oldStatus = order.CurrentStatus;
        order.CurrentStatus = newStatus;

        if (newStatus == OrderStatus.Issued)
        {
            order.IssuedAt = DateTime.Now;
            if (order.CellId.HasValue)
            {
                var cell = await db.StorageCells.FindAsync(order.CellId);
                if (cell != null) cell.IsBusy = false;
            }
        }
        else if (newStatus == OrderStatus.Returned)
        {
            if (order.CellId.HasValue)
            {
                var cell = await db.StorageCells.FindAsync(order.CellId);
                if (cell != null) cell.IsBusy = false;
            }
        }

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.Now,
            EmployeeId = employeeId
        });

        await db.SaveChangesAsync();
    }

    public async Task<IssueOperation> IssueOrderAsync(int orderId, int employeeId, int shiftId, IssueResult result, decimal totalAmount, string? comment = null, List<OrderItem>? issuedItems = null)
    {
        using var db = new AppDbContext();

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new InvalidOperationException("Заказ не найден");

        var oldStatus = order.CurrentStatus;

        if (result == IssueResult.Partial && issuedItems != null)
        {
            foreach (var item in order.Items)
            {
                var isIssued = issuedItems.Any(i => i.Id == item.Id);
                item.IsIssued = isIssued;
            }
            order.CurrentStatus = OrderStatus.PartialIssued;
        }
        else
        {
            order.CurrentStatus = OrderStatus.Issued;
            foreach (var item in order.Items)
                item.IsIssued = true;
        }

        order.IssuedAt = DateTime.Now;
        if (order.CellId.HasValue)
        {
            var cell = await db.StorageCells.FindAsync(order.CellId);
            if (cell != null) cell.IsBusy = false;
        }

        var operation = new IssueOperation
        {
            OrderId = orderId,
            EmployeeId = employeeId,
            ShiftId = shiftId,
            IssueDateTime = DateTime.Now,
            Result = result,
            TotalAmount = totalAmount,
            Comment = comment
        };
        db.IssueOperations.Add(operation);

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = order.CurrentStatus,
            ChangedAt = DateTime.Now,
            EmployeeId = employeeId
        });

        await db.SaveChangesAsync();
        return operation;
    }

    public async Task<ReturnOperation> ReturnOrderAsync(int orderId, int employeeId, int shiftId, string reason)
    {
        using var db = new AppDbContext();
        var order = await db.Orders.FindAsync(orderId);
        if (order == null) throw new InvalidOperationException("Заказ не найден");

        var oldStatus = order.CurrentStatus;
        order.CurrentStatus = OrderStatus.Returned;

        if (order.CellId.HasValue)
        {
            var cell = await db.StorageCells.FindAsync(order.CellId);
            if (cell != null) cell.IsBusy = false;
        }

        var operation = new ReturnOperation
        {
            OrderId = orderId,
            EmployeeId = employeeId,
            ShiftId = shiftId,
            Reason = reason,
            ReturnDateTime = DateTime.Now
        };
        db.ReturnOperations.Add(operation);

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = orderId,
            OldStatus = oldStatus,
            NewStatus = OrderStatus.Returned,
            ChangedAt = DateTime.Now,
            EmployeeId = employeeId
        });

        await db.SaveChangesAsync();
        return operation;
    }

    public async Task<List<Order>> SearchByPhoneAsync(string phone)
    {
        using var db = new AppDbContext();
        var cleanPhone = new string(phone.Where(c => char.IsDigit(c)).ToArray());
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Where(o => o.Client.Phone.Contains(cleanPhone))
            .OrderByDescending(o => o.CreatedAt)
            .Take(20)
            .ToListAsync();
    }

    public async Task<Client?> FindClientByPhoneAsync(string phone)
    {
        using var db = new AppDbContext();
        var cleanPhone = new string(phone.Where(c => char.IsDigit(c)).ToArray());
        return await db.Clients.FirstOrDefaultAsync(c => c.Phone.Contains(cleanPhone));
    }

    private string GenerateBarcode()
    {
        return $"PVZ{DateTime.Now:yyyyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}

public class DashboardStats
{
    public int OrdersInStorage { get; set; }
    public int OverdueOrders { get; set; }
    public int IssuedToday { get; set; }
    public int ReturnsToday { get; set; }
}