using Microsoft.EntityFrameworkCore;
using AISPVZ.Models;
using System.IO;

namespace AISPVZ.Data.Context;

public class AppDbContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<StorageCell> StorageCells => Set<StorageCell>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<IssueOperation> IssueOperations => Set<IssueOperation>();
    public DbSet<ReturnOperation> ReturnOperations => Set<ReturnOperation>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    private static string DbPath
    {
        get
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AISPVZ");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "aispvz.db");
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var appDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AISPVZ");
        Directory.CreateDirectory(appDataFolder);
        var dbPath = Path.Combine(appDataFolder, "aispvz.db");
        options.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Unique indexes
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Login)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Barcode)
            .IsUnique();

        modelBuilder.Entity<StorageCell>()
            .HasIndex(c => c.CellCode)
            .IsUnique();

        modelBuilder.Entity<SystemSetting>()
            .HasIndex(s => s.Key)
            .IsUnique();

        // Relationships
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Client)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Cell)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CellId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderStatusHistory>()
            .HasOne(osh => osh.Order)
            .WithMany(o => o.StatusHistories)
            .HasForeignKey(osh => osh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Shift>()
            .HasOne(s => s.Employee)
            .WithMany(e => e.Shifts)
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<IssueOperation>()
            .HasOne(io => io.Order)
            .WithMany(o => o.IssueOperations)
            .HasForeignKey(io => io.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<IssueOperation>()
            .HasOne(io => io.Employee)
            .WithMany(e => e.IssueOperations)
            .HasForeignKey(io => io.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReturnOperation>()
            .HasOne(ro => ro.Order)
            .WithMany(o => o.ReturnOperations)
            .HasForeignKey(ro => ro.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReturnOperation>()
            .HasOne(ro => ro.Employee)
            .WithMany(e => e.ReturnOperations)
            .HasForeignKey(ro => ro.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}