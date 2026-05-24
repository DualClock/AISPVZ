using AISPVZ.Data.Context;
using AISPVZ.Models;
using Microsoft.EntityFrameworkCore;

namespace AISPVZ.Services;

public class ReferenceService
{
    // Storage Cells
    public async Task<List<StorageCell>> GetAllCellsAsync()
    {
        using var db = new AppDbContext();
        return await db.StorageCells.OrderBy(c => c.Zone).ThenBy(c => c.CellCode).ToListAsync();
    }

    public async Task<List<StorageCell>> GetFreeCellsAsync(string? zone = null)
    {
        using var db = new AppDbContext();
        var query = db.StorageCells.Where(c => !c.IsBusy);
        if (!string.IsNullOrEmpty(zone))
            query = query.Where(c => c.Zone == zone);
        return await query.OrderBy(c => c.Zone).ThenBy(c => c.CellCode).ToListAsync();
    }

    public async Task<StorageCell?> GetCellByCodeAsync(string code)
    {
        using var db = new AppDbContext();
        return await db.StorageCells.FirstOrDefaultAsync(c => c.CellCode == code);
    }

    public async Task<StorageCell> CreateCellAsync(StorageCell cell)
    {
        using var db = new AppDbContext();
        db.StorageCells.Add(cell);
        await db.SaveChangesAsync();
        return cell;
    }

    public async Task UpdateCellAsync(StorageCell cell)
    {
        using var db = new AppDbContext();
        db.StorageCells.Update(cell);
        await db.SaveChangesAsync();
    }

    // Employees
    public async Task<List<Employee>> GetAllEmployeesAsync()
    {
        using var db = new AppDbContext();
        return await db.Employees.OrderBy(e => e.FullName).ToListAsync();
    }

    public async Task<List<Employee>> GetActiveEmployeesAsync()
    {
        using var db = new AppDbContext();
        return await db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        using var db = new AppDbContext();
        db.Employees.Add(employee);
        await db.SaveChangesAsync();
        return employee;
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        using var db = new AppDbContext();
        db.Employees.Update(employee);
        await db.SaveChangesAsync();
    }

    public async Task DeactivateEmployeeAsync(int id)
    {
        using var db = new AppDbContext();
        var emp = await db.Employees.FindAsync(id);
        if (emp != null)
        {
            emp.IsActive = false;
            await db.SaveChangesAsync();
        }
    }

    // Clients
    public async Task<List<Client>> GetAllClientsAsync()
    {
        using var db = new AppDbContext();
        return await db.Clients.OrderBy(c => c.FullName).ToListAsync();
    }

    public async Task<List<Client>> SearchClientsAsync(string query)
    {
        using var db = new AppDbContext();
        var q = query.ToLower();
        return await db.Clients
            .Where(c => c.FullName.ToLower().Contains(q) || c.Phone.Contains(q))
            .OrderBy(c => c.FullName)
            .Take(50)
            .ToListAsync();
    }

    public async Task<Client> CreateClientAsync(Client client)
    {
        using var db = new AppDbContext();
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return client;
    }

    public async Task UpdateClientAsync(Client client)
    {
        using var db = new AppDbContext();
        db.Clients.Update(client);
        await db.SaveChangesAsync();
    }

    // System Settings
    public async Task<string?> GetSettingAsync(string key)
    {
        using var db = new AppDbContext();
        return await db.SystemSettings.Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync();
    }

    public async Task SetSettingAsync(string key, string value)
    {
        using var db = new AppDbContext();
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting != null)
        {
            setting.Value = value;
        }
        else
        {
            db.SystemSettings.Add(new SystemSetting { Key = key, Value = value });
        }
        await db.SaveChangesAsync();
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        using var db = new AppDbContext();
        return await db.SystemSettings.ToDictionaryAsync(s => s.Key, s => s.Value);
    }

    public async Task<List<string>> GetReturnReasonsAsync()
    {
        using var db = new AppDbContext();
        return await db.SystemSettings
            .Where(s => s.Key.StartsWith("Reason_"))
            .Select(s => s.Value)
            .ToListAsync();
    }
}