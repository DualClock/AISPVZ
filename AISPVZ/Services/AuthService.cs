using AISPVZ.Data.Context;
using AISPVZ.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Authentication;

namespace AISPVZ.Services;

public class AuthService
{
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 5;

    private static readonly Dictionary<string, (int attempts, DateTime? lockedUntil)> _failedAttempts = new();
    private static readonly object _lockObj = new();

    public async Task<Employee> LoginAsync(string login, string password)
    {
        string key = login.ToLower();

        lock (_lockObj)
        {
            ClearExpiredLockouts();
        }

        lock (_lockObj)
        {
            if (_failedAttempts.TryGetValue(key, out var info) && info.lockedUntil.HasValue)
            {
                if (DateTime.Now < info.lockedUntil.Value)
                {
                    var remaining = (info.lockedUntil.Value - DateTime.Now).Minutes + 1;
                    throw new AuthenticationException($"Учётная запись заблокирована. Попробуйте через {remaining} мин.");
                }
                _failedAttempts.Remove(key);
            }
        }

        using var db = new AppDbContext();
        var employee = await db.Employees.FirstOrDefaultAsync(e => e.Login.ToLower() == login.ToLower() && e.IsActive);

        if (employee == null || employee.PasswordHash != password)
        {
            lock (_lockObj)
            {
                RecordFailedAttempt(key);
                var attemptsLeft = MaxFailedAttempts - (_failedAttempts.TryGetValue(key, out var i) ? i.attempts : 0);
                throw new AuthenticationException($"Неверный логин или пароль. Осталось попыток: {attemptsLeft}");
            }
        }

        lock (_lockObj)
        {
            _failedAttempts.Remove(key);
        }
        return employee;
    }

    private void RecordFailedAttempt(string key)
    {
        if (!_failedAttempts.TryGetValue(key, out var info))
        {
            info = (0, null);
        }
        info.attempts++;
        if (info.attempts >= MaxFailedAttempts)
        {
            info.lockedUntil = DateTime.Now.AddMinutes(LockoutMinutes);
        }
        _failedAttempts[key] = info;
    }

    private void ClearExpiredLockouts()
    {
        var expired = _failedAttempts.Where(kvp => kvp.Value.lockedUntil.HasValue && kvp.Value.lockedUntil.Value < DateTime.Now).Select(kvp => kvp.Key).ToList();
        foreach (var key in expired)
        {
            _failedAttempts.Remove(key);
        }
    }

    public void ChangePassword(int employeeId, string newPassword)
    {
        using var db = new AppDbContext();
        var employee = db.Employees.Find(employeeId);
        if (employee != null)
        {
            employee.PasswordHash = newPassword;
            db.SaveChanges();
        }
    }
}