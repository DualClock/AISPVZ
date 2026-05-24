using Microsoft.EntityFrameworkCore;
using AISPVZ.Models;

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

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=AISPVZ_DB;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=""AISPVZ"";Command Timeout=30");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.Entity<Employee>().ToTable("Employees");
        modelBuilder.Entity<Client>().ToTable("Clients");
        modelBuilder.Entity<StorageCell>().ToTable("StorageCells");
        modelBuilder.Entity<Order>().ToTable("Orders");
        modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
        modelBuilder.Entity<Shift>().ToTable("Shifts");
        modelBuilder.Entity<IssueOperation>().ToTable("IssueOperations");
        modelBuilder.Entity<ReturnOperation>().ToTable("ReturnOperations");
        modelBuilder.Entity<OrderStatusHistory>().ToTable("OrderStatusHistory");
        modelBuilder.Entity<SystemSetting>().ToTable("SystemSettings");

        
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Login)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Barcode)
            .IsUnique();

        modelBuilder.Entity<StorageCell>()
            .HasIndex(c => c.CellCode)
            .IsUnique();

        modelBuilder.Entity<StorageCell>()
            .Property(c => c.MaxWeightKg)
            .HasColumnType("decimal(18,2)");

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
            .Property(oi => oi.Price)
            .HasColumnType("decimal(18,2)");

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
            .Property(io => io.TotalAmount)
            .HasColumnType("decimal(18,2)");

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

        modelBuilder.Entity<ReturnOperation>()
            .HasOne(ro => ro.Shift)
            .WithMany(s => s.ReturnOperations)
            .HasForeignKey(ro => ro.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}