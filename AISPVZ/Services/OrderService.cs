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
            .Include(o => o.Items)
            .Where(o => o.CurrentStatus == OrderStatus.InStorage || o.CurrentStatus == OrderStatus.Accepted)
            .OrderBy(o => o.PlannedIssueDate)
            .ToListAsync();
    }

    public async Task<List<Order>> GetActiveOrdersAsync()
    {
        return await GetPendingOrdersAsync();
    }

    public async Task<List<Order>> GetInStorageOrdersAsync()
    {
        using var db = new AppDbContext();
        return await db.Orders
            .Include(o => o.Client)
            .Include(o => o.Cell)
            .Where(o => o.CurrentStatus == OrderStatus.InStorage)
            .OrderBy(o => o.PlannedIssueDate)
            .ToListAsync();
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

        int clientId = order.ClientId;
        if (clientId == 0)
        {
            string? clientName = order.Client?.FullName;
            string? clientPhone = order.Client?.Phone;
            string? clientEmail = order.Client?.Email;

            if (!string.IsNullOrWhiteSpace(clientPhone))
            {
                var existingClient = await db.Clients.FirstOrDefaultAsync(c => c.Phone == clientPhone);
                if (existingClient != null)
                {
                    clientId = existingClient.Id;
                }
                else
                {
                    var newClient = new Client
                    {
                        FullName = clientName ?? "Новый клиент",
                        Phone = clientPhone,
                        Email = clientEmail
                    };
                    db.Clients.Add(newClient);
                    await db.SaveChangesAsync();
                    clientId = newClient.Id;
                }
            }
            else
            {
                var newClient = new Client
                {
                    FullName = clientName ?? "Новый клиент",
                    Phone = "",
                    Email = clientEmail
                };
                db.Clients.Add(newClient);
                await db.SaveChangesAsync();
                clientId = newClient.Id;
            }
        }

        var newOrder = new Order
        {
            ClientId = clientId,
            CellId = order.CellId,
            Barcode = order.Barcode,
            Marketplace = order.Marketplace,
            PlannedIssueDate = order.PlannedIssueDate,
            Comment = order.Comment,
            CurrentStatus = OrderStatus.InStorage,
            ArrivedAt = DateTime.Now
        };

        if (newOrder.CellId == null)
        {
            var cell = await db.StorageCells.FirstOrDefaultAsync(c => !c.IsBusy);
            if (cell != null)
            {
                cell.IsBusy = true;
                newOrder.CellId = cell.Id;
            }
        }
        else
        {
            var cell = await db.StorageCells.FindAsync(newOrder.CellId);
            if (cell != null)
                cell.IsBusy = true;
        }

        if (string.IsNullOrWhiteSpace(newOrder.Barcode))
            newOrder.Barcode = GenerateBarcode();

        db.Orders.Add(newOrder);
        await db.SaveChangesAsync();

        foreach (var item in items)
        {
            item.OrderId = newOrder.Id;
            db.OrderItems.Add(item);
        }
        await db.SaveChangesAsync();

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = newOrder.Id,
            OldStatus = OrderStatus.Pending,
            NewStatus = OrderStatus.InStorage,
            ChangedAt = DateTime.Now
        });
        await db.SaveChangesAsync();

        return newOrder;
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
        if (shiftId == 0)
            throw new InvalidOperationException("Невозможно выполнить выдачу: смена не открыта");
        if (employeeId == 0)
            throw new InvalidOperationException("Невозможно выполнить выдачу: сотрудник не определен");

        using var db = new AppDbContext();

        var shift = await db.Shifts.FindAsync(shiftId);
        if (shift == null)
            throw new InvalidOperationException("Смена не найдена");

        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new InvalidOperationException("Заказ не найден");

        var oldStatus = order.CurrentStatus;

        if (result == IssueResult.Partial && issuedItems != null && issuedItems.Count > 0)
        {
            foreach (var item in order.Items)
            {
                var isIssued = issuedItems.Any(i => i.ProductName == item.ProductName && i.Price == item.Price);
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
        if (shiftId == 0)
            throw new InvalidOperationException("Невозможно выполнить возврат: смена не открыта");
        if (employeeId == 0)
            throw new InvalidOperationException("Невозможно выполнить возврат: сотрудник не определен");
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Укажите причину возврата");

        using var db = new AppDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var shift = await db.Shifts.FindAsync(shiftId);
            if (shift == null)
                throw new InvalidOperationException("Смена не найдена");

            var employee = await db.Employees.FindAsync(employeeId);
            if (employee == null)
                throw new InvalidOperationException("Сотрудник не найден");

            var order = await db.Orders.FindAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Заказ не найден");

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
                ReturnDateTime = DateTime.Now,
                ReturnToMarketplaceDate = null
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
            await transaction.CommitAsync();
            return operation;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var msg = ex.Message;
            if (ex.InnerException != null)
                msg += " | Inner: " + ex.InnerException.Message;
            throw new InvalidOperationException("Ошибка при сохранении возврата: " + msg, ex);
        }
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

    public async Task<string> CreateOrderSimpleAsync(
        string clientName,
        string clientPhone,
        string clientEmail,
        int? cellId,
        string barcode,
        Marketplace marketplace,
        string comment,
        List<OrderItem> items,
        int? maxStorageDays = null)
    {
        using var db = new AppDbContext();

        int clientId;
        var cleanPhone = new string(clientPhone.Where(c => char.IsDigit(c)).ToArray());

        if (!string.IsNullOrWhiteSpace(cleanPhone))
        {
            var existingClient = await db.Clients.FirstOrDefaultAsync(c => c.Phone.Contains(cleanPhone));
            if (existingClient != null)
            {
                clientId = existingClient.Id;
            }
            else
            {
                var newClient = new Client
                {
                    FullName = string.IsNullOrWhiteSpace(clientName) ? "Новый клиент" : clientName,
                    Phone = clientPhone,
                    Email = clientEmail
                };
                db.Clients.Add(newClient);
                await db.SaveChangesAsync();
                clientId = newClient.Id;
            }
        }
        else
        {
            var newClient = new Client
            {
                FullName = string.IsNullOrWhiteSpace(clientName) ? "Новый клиент" : clientName,
                Phone = "",
                Email = clientEmail
            };
            db.Clients.Add(newClient);
            await db.SaveChangesAsync();
            clientId = newClient.Id;
        }

        int? finalCellId = cellId;
        if (finalCellId == null)
        {
            var freeCell = await db.StorageCells.FirstOrDefaultAsync(c => !c.IsBusy);
            if (freeCell != null)
            {
                freeCell.IsBusy = true;
                finalCellId = freeCell.Id;
            }
        }
        else
        {
            var cell = await db.StorageCells.FindAsync(finalCellId);
            if (cell != null)
                cell.IsBusy = true;
        }

        string finalBarcode = barcode;
        if (string.IsNullOrWhiteSpace(finalBarcode))
            finalBarcode = GenerateBarcode();

        int storageDays = maxStorageDays ?? 7;
        if (storageDays < 1) storageDays = 7;

        var order = new Order
        {
            ClientId = clientId,
            CellId = finalCellId,
            Barcode = finalBarcode,
            Marketplace = marketplace,
            CurrentStatus = OrderStatus.InStorage,
            ArrivedAt = DateTime.Now,
            PlannedIssueDate = DateTime.Now.AddDays(storageDays),
            Comment = comment
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var itemCopies = items.Select(i => new OrderItem
        {
            OrderId = order.Id,
            Article = i.Article ?? "",
            ProductName = i.ProductName ?? "Товар",
            Quantity = i.Quantity,
            Price = i.Price,
            IsIssued = false
        }).ToList();

        foreach (var item in itemCopies)
        {
            db.OrderItems.Add(item);
        }
        await db.SaveChangesAsync();

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            OldStatus = OrderStatus.Pending,
            NewStatus = OrderStatus.InStorage,
            ChangedAt = DateTime.Now
        });
        await db.SaveChangesAsync();

        return finalBarcode;
    }
}

public class DashboardStats
{
    public int OrdersInStorage { get; set; }
    public int OverdueOrders { get; set; }
    public int IssuedToday { get; set; }
    public int ReturnsToday { get; set; }
}
