using AISPVZ.Data.Context;
using AISPVZ.Models;
using Microsoft.EntityFrameworkCore;

namespace AISPVZ.Services;

public class ShiftService
{
    public async Task<Shift?> GetOpenShiftAsync(int employeeId)
    {
        using var db = new AppDbContext();
        return await db.Shifts.FirstOrDefaultAsync(s => s.EmployeeId == employeeId && !s.IsClosed);
    }

    public async Task<Shift> OpenShiftAsync(int employeeId)
    {
        using var db = new AppDbContext();

        var existingShifts = await db.Shifts.Where(s => s.EmployeeId == employeeId && !s.IsClosed).ToListAsync();
        foreach (var existingShift in existingShifts)
        {
            existingShift.IsClosed = true;
            existingShift.EndTime = DateTime.Now;
        }

        var newShift = new Shift
        {
            EmployeeId = employeeId,
            StartTime = DateTime.Now,
            IsClosed = false
        };
        db.Shifts.Add(newShift);
        await db.SaveChangesAsync();
        return newShift;
    }

    public async Task CloseShiftAsync(int shiftId)
    {
        using var db = new AppDbContext();
        var shiftEntity = await db.Shifts.FindAsync(shiftId);
        if (shiftEntity != null)
        {
            shiftEntity.IsClosed = true;
            shiftEntity.EndTime = DateTime.Now;
            await db.SaveChangesAsync();
        }
    }

    public async Task<int> GetOverdueOrdersCountAsync()
    {
        using var db = new AppDbContext();
        var now = DateTime.Now.Date;
        return await db.Orders.CountAsync(o =>
            o.CurrentStatus == OrderStatus.InStorage &&
            o.PlannedIssueDate < now);
    }

    public async Task<(int issued, int returns, decimal totalAmount)> GetShiftSummaryAsync(int shiftId)
    {
        using var db = new AppDbContext();
        var issues = await db.IssueOperations.Where(io => io.ShiftId == shiftId).ToListAsync();
        var returns = await db.ReturnOperations.CountAsync(r => r.ShiftId == shiftId);
        return (issues.Count, returns, issues.Sum(i => i.TotalAmount));
    }
}