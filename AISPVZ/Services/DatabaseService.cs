using AISPVZ.Data.Context;
using AISPVZ.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AISPVZ.Services;

public class DatabaseService
{
    private readonly string _backupsFolder;

    public DatabaseService()
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AISPVZ");
        Directory.CreateDirectory(appDataFolder);
        _backupsFolder = Path.Combine(appDataFolder, "Backups");
    }

    public async Task InitializeDatabaseAsync()
    {
        using var db = new AppDbContext();
        var created = await db.Database.EnsureCreatedAsync();
        if (created)
        {
            await SeedDataAsync();
        }
    }

    public async Task CreateDatabaseAsync()
    {
        using var db = new AppDbContext();
        await db.Database.EnsureCreatedAsync();
    }

    public async Task SeedDataAsync()
    {
        using var db = new AppDbContext();

        db.OrderStatusHistories.RemoveRange(db.OrderStatusHistories);
        db.ReturnOperations.RemoveRange(db.ReturnOperations);
        db.IssueOperations.RemoveRange(db.IssueOperations);
        db.OrderItems.RemoveRange(db.OrderItems);
        db.Shifts.RemoveRange(db.Shifts);
        db.Orders.RemoveRange(db.Orders);
        db.StorageCells.RemoveRange(db.StorageCells);
        db.Clients.RemoveRange(db.Clients);
        db.Employees.RemoveRange(db.Employees);
        db.SystemSettings.RemoveRange(db.SystemSettings);
        await db.SaveChangesAsync();

        var employees = new List<Employee>
        {
            new() { FullName = "Иванов Иван Иванович", Login = "ivanov", PasswordHash = "ivanov123", Role = EmployeeRole.Operator, IsActive = true },
            new() { FullName = "Петрова Мария Сергеевна", Login = "petrova", PasswordHash = "petrova123", Role = EmployeeRole.Admin, IsActive = true },
            new() { FullName = "Сидоров Алексей Владимирович", Login = "sidorov", PasswordHash = "sidorov123", Role = EmployeeRole.Operator, IsActive = true },
            new() { FullName = "Козлова Екатерина Андреевна", Login = "kozlova", PasswordHash = "kozlova123", Role = EmployeeRole.Operator, IsActive = true },
            new() { FullName = "Морозов Дмитрий Петрович", Login = "morozov", PasswordHash = "morozov123", Role = EmployeeRole.Operator, IsActive = false }
        };
        db.Employees.AddRange(employees);
        await db.SaveChangesAsync();

        var clients = new List<Client>
        {
            new() { FullName = "ООО \"Рога и Копыта\"", Phone = "+7(495)123-45-67", Email = "info@roga.ru" },
            new() { FullName = "ИП Смирнов А.Б.", Phone = "+7(916)789-12-34", Email = "smirnov@mail.ru" },
            new() { FullName = "ЗАО \"ТехноСервис\"", Phone = "+7(812)345-67-89", Email = "tech@service.com" },
            new() { FullName = "ООО \"СтройМаркет\"", Phone = "+7(343)567-89-01", Email = "stroy@market.ru" },
            new() { FullName = "ИП Кузнецов В.В.", Phone = "+7(926)234-56-78", Email = "kuznetsov@bk.ru" },
            new() { FullName = "ОАО \"Торговый Дом\"", Phone = "+7(495)987-65-43", Email = "td@td.ru" },
            new() { FullName = "Сидоренко Андрей Петрович", Phone = "+7(903)111-22-33", Email = "asidorenko@mail.com" },
            new() { FullName = "ООО \"Электроника Плюс\"", Phone = "+7(383)222-33-44", Email = "electron@plus.ru" }
        };
        db.Clients.AddRange(clients);
        await db.SaveChangesAsync();

        var cells = new List<StorageCell>
        {
            new() { CellCode = "A-01", Zone = "A", IsBusy = false, MaxWeightKg = 30.0m, Comment = "Маленькая ячейка" },
            new() { CellCode = "A-02", Zone = "A", IsBusy = true, MaxWeightKg = 30.0m, Comment = "Средняя ячейка" },
            new() { CellCode = "A-03", Zone = "A", IsBusy = false, MaxWeightKg = 50.0m, Comment = "Большая ячейка" },
            new() { CellCode = "B-01", Zone = "B", IsBusy = true, MaxWeightKg = 100.0m, Comment = "Для тяжелых грузов" },
            new() { CellCode = "B-02", Zone = "B", IsBusy = false, MaxWeightKg = 100.0m, Comment = null },
            new() { CellCode = "B-03", Zone = "B", IsBusy = true, MaxWeightKg = 150.0m, Comment = "VIP ячейка" },
            new() { CellCode = "C-01", Zone = "C", IsBusy = false, MaxWeightKg = 30.0m, Comment = "Стеллаж 1" },
            new() { CellCode = "C-02", Zone = "C", IsBusy = true, MaxWeightKg = 30.0m, Comment = "Стеллаж 2" },
            new() { CellCode = "C-03", Zone = "C", IsBusy = false, MaxWeightKg = 50.0m, Comment = null },
            new() { CellCode = "D-01", Zone = "D", IsBusy = true, MaxWeightKg = 200.0m, Comment = "Для крупногабарита" }
        };
        db.StorageCells.AddRange(cells);
        await db.SaveChangesAsync();

        var shifts = new List<Shift>
        {
            new() { EmployeeId = 1, StartTime = DateTime.Now.AddHours(-8), EndTime = DateTime.Now, IsClosed = true },
            new() { EmployeeId = 2, StartTime = DateTime.Now.AddDays(-1).AddHours(9), EndTime = DateTime.Now.AddDays(-1).AddHours(18), IsClosed = true },
            new() { EmployeeId = 3, StartTime = DateTime.Now.AddDays(-2).AddHours(10), EndTime = DateTime.Now.AddDays(-2).AddHours(19), IsClosed = true },
            new() { EmployeeId = 1, StartTime = DateTime.Now.AddHours(-4), EndTime = null, IsClosed = false },
            new() { EmployeeId = 4, StartTime = DateTime.Now.AddDays(-3).AddHours(8), EndTime = DateTime.Now.AddDays(-3).AddHours(17), IsClosed = true },
            new() { EmployeeId = 2, StartTime = DateTime.Now.AddHours(-6), EndTime = null, IsClosed = false }
        };
        db.Shifts.AddRange(shifts);
        await db.SaveChangesAsync();

        var orders = new List<Order>
        {
            new() { ClientId = 1, CellId = null, Barcode = "BARCODE001", Marketplace = Marketplace.Wildberries, CurrentStatus = OrderStatus.Issued, ArrivedAt = DateTime.Now.AddDays(-10), PlannedIssueDate = DateTime.Now.AddDays(-5), IssuedAt = DateTime.Now.AddDays(-5), PhotoPath = "/photos/order1.jpg", Comment = "Срочный заказ" },
            new() { ClientId = 4, CellId = null, Barcode = "BARCODE004", Marketplace = Marketplace.Ozon, CurrentStatus = OrderStatus.Issued, ArrivedAt = DateTime.Now.AddDays(-15), PlannedIssueDate = DateTime.Now.AddDays(-10), IssuedAt = DateTime.Now.AddDays(-10), PhotoPath = "/photos/order4.jpg", Comment = "Выдан клиенту" },
            new() { ClientId = 7, CellId = null, Barcode = "BARCODE008", Marketplace = Marketplace.Wildberries, CurrentStatus = OrderStatus.Issued, ArrivedAt = DateTime.Now.AddDays(-12), PlannedIssueDate = DateTime.Now.AddDays(-8), IssuedAt = DateTime.Now.AddDays(-8), PhotoPath = "/photos/order8.jpg", Comment = "Выдан" },

            new() { ClientId = 2, CellId = 2, Barcode = "BARCODE002", Marketplace = Marketplace.Wildberries, CurrentStatus = OrderStatus.InStorage, ArrivedAt = DateTime.Now.AddDays(-8), PlannedIssueDate = DateTime.Now.AddDays(2), IssuedAt = null, PhotoPath = null, Comment = "Ожидает выдачи" },
            new() { ClientId = 1, CellId = 6, Barcode = "BARCODE006", Marketplace = Marketplace.YandexMarket, CurrentStatus = OrderStatus.InStorage, ArrivedAt = DateTime.Now.AddDays(-5), PlannedIssueDate = DateTime.Now.AddDays(5), IssuedAt = null, PhotoPath = null, Comment = "Ждет в ячейке" },
            new() { ClientId = 8, CellId = 8, Barcode = "BARCODE009", Marketplace = Marketplace.YandexMarket, CurrentStatus = OrderStatus.InStorage, ArrivedAt = DateTime.Now.AddDays(-7), PlannedIssueDate = DateTime.Now.AddDays(3), IssuedAt = null, PhotoPath = null, Comment = "В обработке" },

            new() { ClientId = 5, CellId = null, Barcode = "BARCODE005", Marketplace = Marketplace.Wildberries, CurrentStatus = OrderStatus.Returned, ArrivedAt = DateTime.Now.AddDays(-20), PlannedIssueDate = DateTime.Now.AddDays(-15), IssuedAt = null, PhotoPath = "/photos/order5.jpg", Comment = "Возврат" },

            new() { ClientId = 3, CellId = null, Barcode = "BARCODE003", Marketplace = Marketplace.YandexMarket, CurrentStatus = OrderStatus.Pending, ArrivedAt = DateTime.Now.AddDays(-3), PlannedIssueDate = DateTime.Now.AddDays(7), IssuedAt = null, PhotoPath = null, Comment = "Новый заказ" },
            new() { ClientId = 6, CellId = null, Barcode = "BARCODE007", Marketplace = Marketplace.Ozon, CurrentStatus = OrderStatus.Pending, ArrivedAt = DateTime.Now.AddDays(-2), PlannedIssueDate = DateTime.Now.AddDays(8), IssuedAt = null, PhotoPath = null, Comment = "Только поступил" },
            new() { ClientId = 2, CellId = null, Barcode = "BARCODE010", Marketplace = Marketplace.Ozon, CurrentStatus = OrderStatus.Pending, ArrivedAt = DateTime.Now.AddDays(-1), PlannedIssueDate = DateTime.Now.AddDays(9), IssuedAt = null, PhotoPath = null, Comment = "Срочно" }
        };
        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();

        var items = new List<OrderItem>
        {
            new() { OrderId = 1, ProductName = "Смартфон Xiaomi Note 11", Quantity = 1, Price = 15000.00m, IsIssued = true },
            new() { OrderId = 1, ProductName = "Чехол для смартфона", Quantity = 2, Price = 500.00m, IsIssued = true },
            new() { OrderId = 2, ProductName = "Ноутбук Lenovo IdeaPad", Quantity = 1, Price = 45000.00m, IsIssued = false },
            new() { OrderId = 3, ProductName = "Наушники Sony WH-1000XM4", Quantity = 1, Price = 25000.00m, IsIssued = false },
            new() { OrderId = 3, ProductName = "Зарядное устройство", Quantity = 1, Price = 1500.00m, IsIssued = false },
            new() { OrderId = 4, ProductName = "Клавиатура Logitech", Quantity = 1, Price = 3500.00m, IsIssued = true },
            new() { OrderId = 4, ProductName = "Мышь Logitech", Quantity = 1, Price = 1200.00m, IsIssued = true },
            new() { OrderId = 5, ProductName = "Монитор Samsung 24\"", Quantity = 2, Price = 12000.00m, IsIssued = false },
            new() { OrderId = 6, ProductName = "Внешний жесткий диск 1TB", Quantity = 1, Price = 5000.00m, IsIssued = false },
            new() { OrderId = 6, ProductName = "Флешка 64GB", Quantity = 3, Price = 800.00m, IsIssued = false },
            new() { OrderId = 7, ProductName = "Планшет iPad 10.2", Quantity = 1, Price = 30000.00m, IsIssued = false },
            new() { OrderId = 8, ProductName = "Смарт-часы Apple Watch", Quantity = 1, Price = 28000.00m, IsIssued = true },
            new() { OrderId = 9, ProductName = "Фитнес-браслет Xiaomi", Quantity = 2, Price = 2500.00m, IsIssued = false },
            new() { OrderId = 9, ProductName = "Беспроводная зарядка", Quantity = 1, Price = 1200.00m, IsIssued = false },
            new() { OrderId = 10, ProductName = "Роутер TP-Link", Quantity = 1, Price = 3500.00m, IsIssued = false }
        };
        db.OrderItems.AddRange(items);
        await db.SaveChangesAsync();

        var issueOps = new List<IssueOperation>
        {
            new() { OrderId = 1, EmployeeId = 1, ShiftId = 1, IssueDateTime = DateTime.Now.AddDays(-5), Result = IssueResult.Issued, TotalAmount = 16000.00m, Comment = "Выдано полностью" },
            new() { OrderId = 4, EmployeeId = 2, ShiftId = 2, IssueDateTime = DateTime.Now.AddDays(-10), Result = IssueResult.Issued, TotalAmount = 4700.00m, Comment = "Выдано без замечаний" },
            new() { OrderId = 8, EmployeeId = 3, ShiftId = 3, IssueDateTime = DateTime.Now.AddDays(-8), Result = IssueResult.Issued, TotalAmount = 28000.00m, Comment = "Частичная выдача" },
            new() { OrderId = 7, EmployeeId = 1, ShiftId = 4, IssueDateTime = DateTime.Now.AddDays(-2), Result = IssueResult.Refused, TotalAmount = 0, Comment = "Клиент не пришел" },
            new() { OrderId = 2, EmployeeId = 4, ShiftId = 5, IssueDateTime = DateTime.Now.AddDays(-6), Result = IssueResult.Issued, TotalAmount = 45000.00m, Comment = "Выдано успешно" }
        };
        db.IssueOperations.AddRange(issueOps);
        await db.SaveChangesAsync();

        var returnOps = new List<ReturnOperation>
        {
            new() { OrderId = 5, EmployeeId = 2, ShiftId = 2, Reason = "Товар не подошел по характеристикам", ReturnDateTime = DateTime.Now.AddDays(-18), ReturnToMarketplaceDate = DateTime.Now.AddDays(-15) },
            new() { OrderId = 3, EmployeeId = 1, ShiftId = 1, Reason = "Брак в товаре", ReturnDateTime = DateTime.Now.AddDays(-2), ReturnToMarketplaceDate = DateTime.Now.AddDays(3) },
            new() { OrderId = 8, EmployeeId = 3, ShiftId = 3, Reason = "Передумал покупать", ReturnDateTime = DateTime.Now.AddDays(-7), ReturnToMarketplaceDate = DateTime.Now.AddDays(-5) },
            new() { OrderId = 6, EmployeeId = 4, ShiftId = 5, Reason = "Неверная комплектация", ReturnDateTime = DateTime.Now.AddDays(-4), ReturnToMarketplaceDate = DateTime.Now.AddDays(2) }
        };
        db.ReturnOperations.AddRange(returnOps);
        await db.SaveChangesAsync();

        var histories = new List<OrderStatusHistory>
        {
            new() { OrderId = 1, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Accepted, ChangedAt = DateTime.Now.AddDays(-9), EmployeeId = 1 },
            new() { OrderId = 1, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.InStorage, ChangedAt = DateTime.Now.AddDays(-6), EmployeeId = 2 },
            new() { OrderId = 1, OldStatus = OrderStatus.InStorage, NewStatus = OrderStatus.Issued, ChangedAt = DateTime.Now.AddDays(-5), EmployeeId = 1 },
            new() { OrderId = 2, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Accepted, ChangedAt = DateTime.Now.AddDays(-7), EmployeeId = 3 },
            new() { OrderId = 3, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Accepted, ChangedAt = DateTime.Now.AddDays(-2), EmployeeId = 1 },
            new() { OrderId = 4, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Accepted, ChangedAt = DateTime.Now.AddDays(-14), EmployeeId = 2 },
            new() { OrderId = 4, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.InStorage, ChangedAt = DateTime.Now.AddDays(-13), EmployeeId = 3 },
            new() { OrderId = 4, OldStatus = OrderStatus.InStorage, NewStatus = OrderStatus.Issued, ChangedAt = DateTime.Now.AddDays(-11), EmployeeId = 1 },
            new() { OrderId = 5, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Accepted, ChangedAt = DateTime.Now.AddDays(-19), EmployeeId = 4 },
            new() { OrderId = 5, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.InStorage, ChangedAt = DateTime.Now.AddDays(-17), EmployeeId = 2 },
            new() { OrderId = 5, OldStatus = OrderStatus.InStorage, NewStatus = OrderStatus.Returned, ChangedAt = DateTime.Now.AddDays(-16), EmployeeId = 1 },
            new() { OrderId = 8, OldStatus = OrderStatus.Pending, NewStatus = OrderStatus.Accepted, ChangedAt = DateTime.Now.AddDays(-11), EmployeeId = 3 },
            new() { OrderId = 8, OldStatus = OrderStatus.Accepted, NewStatus = OrderStatus.InStorage, ChangedAt = DateTime.Now.AddDays(-9), EmployeeId = 2 },
            new() { OrderId = 8, OldStatus = OrderStatus.InStorage, NewStatus = OrderStatus.Issued, ChangedAt = DateTime.Now.AddDays(-8), EmployeeId = 3 }
        };
        db.OrderStatusHistories.AddRange(histories);
        await db.SaveChangesAsync();

        var settings = new List<SystemSetting>
        {
            new() { Key = "CompanyName", Value = "АИС ПВЗ", Description = "Название компании" },
            new() { Key = "WorkStartTime", Value = "09:00", Description = "Начало рабочего дня" },
            new() { Key = "WorkEndTime", Value = "21:00", Description = "Конец рабочего дня" },
            new() { Key = "MaxOrderWeight", Value = "30", Description = "Максимальный вес заказа в кг" },
            new() { Key = "MaxStorageDays", Value = "7", Description = "Максимальный срок хранения в днях" },
            new() { Key = "ReminderHoursBefore", Value = "24", Description = "Напоминание о просрочке за N часов" },
            new() { Key = "ReturnDays", Value = "14", Description = "Срок возврата товара в днях" },
            new() { Key = "AutoBackupEnabled", Value = "true", Description = "Автобэкап при закрытии" },
            new() { Key = "Reason_1", Value = "Товар не подошел по характеристикам", Description = "Причина возврата" },
            new() { Key = "Reason_2", Value = "Брак в товаре", Description = "Причина возврата" },
            new() { Key = "Reason_3", Value = "Передумал покупать", Description = "Причина возврата" },
            new() { Key = "Reason_4", Value = "Неверная комплектация", Description = "Причина возврата" },
            new() { Key = "Reason_5", Value = "Ошибка маркетплейса", Description = "Причина возврата" }
        };
        db.SystemSettings.AddRange(settings);
        await db.SaveChangesAsync();
    }

    public async Task<bool> BackupDatabaseAsync(string? customPath = null)
    {
        try
        {
            Directory.CreateDirectory(_backupsFolder);

            var fileName = customPath ?? Path.Combine(_backupsFolder, $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
            var backupPath = Path.GetFullPath(fileName);

            using var db = new AppDbContext();
            var sql = $"BACKUP DATABASE [AISPVZ_DB] TO DISK = '{backupPath.Replace("'", "''")}' WITH FORMAT, INIT;";
            await db.Database.ExecuteSqlRawAsync(sql);

            var backups = Directory.GetFiles(_backupsFolder, "backup_*.bak")
                .OrderByDescending(f => f)
                .Skip(10)
                .ToList();
            foreach (var old in backups)
                File.Delete(old);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Backup error: {ex}");
            return false;
        }
    }

    public async Task<DateTime?> GetLastBackupTimeAsync()
    {
        using var db = new AppDbContext();
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "LastBackupDate");
        if (setting != null && DateTime.TryParse(setting.Value, out var lastBackup))
            return lastBackup;
        return null;
    }

    public async Task UpdateLastBackupTimeAsync()
    {
        using var db = new AppDbContext();
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == "LastBackupDate");
        if (setting != null)
        {
            setting.Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            db.SystemSettings.Add(new SystemSetting
            {
                Key = "LastBackupDate",
                Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = "Дата последнего резервного копирования"
            });
        }
        await db.SaveChangesAsync();
    }
}
