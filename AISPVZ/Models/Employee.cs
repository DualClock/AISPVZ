using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AISPVZ.Models;

public enum EmployeeRole { Operator, Admin }

public class Employee
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public EmployeeRole Role { get; set; } = EmployeeRole.Operator;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<IssueOperation> IssueOperations { get; set; } = new List<IssueOperation>();
    public virtual ICollection<ReturnOperation> ReturnOperations { get; set; } = new List<ReturnOperation>();
}

public enum ClientRole { Client }

public class Client
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Email { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public class StorageCell
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string CellCode { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Zone { get; set; } = "A";

    public bool IsBusy { get; set; } = false;

    public decimal MaxWeightKg { get; set; } = 30.0m;

    [MaxLength(200)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}

public enum OrderStatus
{
    Pending,       
    Accepted,       
    InStorage,      
    Issued,         
    PartialIssued,  
    Returned,      
    Cancelled       
}

public enum Marketplace
{
    Ozon,
    Wildberries,
    YandexMarket,
    Other
}

public class Order
{
    [Key]
    public int Id { get; set; }

    public int ClientId { get; set; }

    public int? CellId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Barcode { get; set; } = string.Empty;

    public Marketplace Marketplace { get; set; } = Marketplace.Other;

    public OrderStatus CurrentStatus { get; set; } = OrderStatus.Pending;

    public DateTime ArrivedAt { get; set; } = DateTime.Now;

    public DateTime PlannedIssueDate { get; set; } = DateTime.Now.AddDays(7);

    public DateTime? IssuedAt { get; set; }

    [MaxLength(500)]
    public string? PhotoPath { get; set; }

    [MaxLength(500)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [ForeignKey(nameof(ClientId))]
    public virtual Client Client { get; set; } = null!;

    [ForeignKey(nameof(CellId))]
    public virtual StorageCell? Cell { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<IssueOperation> IssueOperations { get; set; } = new List<IssueOperation>();
    public virtual ICollection<ReturnOperation> ReturnOperations { get; set; } = new List<ReturnOperation>();
}

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    [MaxLength(50)]
    public string? Article { get; set; }

    [Required]
    [MaxLength(300)]
    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public decimal Price { get; set; } = 0;

    public bool IsIssued { get; set; } = false;

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;
}

public class Shift
{
    [Key]
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public DateTime StartTime { get; set; } = DateTime.Now;

    public DateTime? EndTime { get; set; }

    public bool IsClosed { get; set; } = false;

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    public virtual ICollection<IssueOperation> IssueOperations { get; set; } = new List<IssueOperation>();
    public virtual ICollection<ReturnOperation> ReturnOperations { get; set; } = new List<ReturnOperation>();

    [NotMapped]
    public string DisplayText => $"{StartTime:dd.MM.yyyy HH:mm} - {(Employee?.FullName ?? "Неизвестен")}";
}

public enum IssueResult { Issued, Partial, Refused }

public class IssueOperation
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int EmployeeId { get; set; }

    public int ShiftId { get; set; }

    public DateTime IssueDateTime { get; set; } = DateTime.Now;

    public IssueResult Result { get; set; } = IssueResult.Issued;

    public decimal TotalAmount { get; set; } = 0;

    [MaxLength(500)]
    public string? Comment { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(ShiftId))]
    public virtual Shift Shift { get; set; } = null!;
}

public class ReturnOperation
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int EmployeeId { get; set; }

    public int ShiftId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Reason { get; set; } = string.Empty;

    public DateTime ReturnDateTime { get; set; } = DateTime.Now;

    public DateTime? ReturnToMarketplaceDate { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(ShiftId))]
    public virtual Shift Shift { get; set; } = null!;
}

public class OrderStatusHistory
{
    [Key]
    public int Id { get; set; }

    public int OrderId { get; set; }

    public OrderStatus OldStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.Now;

    public int? EmployeeId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public virtual Employee? Employee { get; set; }
}

public class SystemSetting
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("SettingKey")]
    public string Key { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("SettingValue")]
    public string Value { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Description { get; set; }
}

public static class EnumHelper
{
    public static Array Marketplaces => Enum.GetValues(typeof(Marketplace));
}