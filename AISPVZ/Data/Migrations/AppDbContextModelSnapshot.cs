
using System;
using AISPVZ.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AISPVZ.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AISPVZ.Models.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("AISPVZ.Models.Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Login")
                        .IsUnique();

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("AISPVZ.Models.IssueOperation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Comment")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<DateTime>("IssueDateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("Result")
                        .HasColumnType("int");

                    b.Property<int>("ShiftId")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("OrderId");

                    b.HasIndex("ShiftId");

                    b.ToTable("IssueOperations");
                });

            modelBuilder.Entity("AISPVZ.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ArrivedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Barcode")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int?>("CellId")
                        .HasColumnType("int");

                    b.Property<int>("ClientId")
                        .HasColumnType("int");

                    b.Property<string>("Comment")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("CurrentStatus")
                        .HasColumnType("int");

                    b.Property<DateTime?>("IssuedAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("Marketplace")
                        .HasColumnType("int");

                    b.Property<string>("PhotoPath")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("PlannedIssueDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Barcode")
                        .IsUnique();

                    b.HasIndex("CellId");

                    b.HasIndex("ClientId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("AISPVZ.Models.OrderItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Article")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsIssued")
                        .HasColumnType("bit");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("AISPVZ.Models.OrderStatusHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("ChangedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<int>("NewStatus")
                        .HasColumnType("int");

                    b.Property<int>("OldStatus")
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("OrderId");

                    b.ToTable("OrderStatusHistories");
                });

            modelBuilder.Entity("AISPVZ.Models.ReturnOperation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("ReturnDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ReturnToMarketplaceDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("ShiftId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("OrderId");

                    b.HasIndex("ShiftId");

                    b.ToTable("ReturnOperations");
                });

            modelBuilder.Entity("AISPVZ.Models.Shift", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("bit");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Shifts");
                });

            modelBuilder.Entity("AISPVZ.Models.StorageCell", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CellCode")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Comment")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsBusy")
                        .HasColumnType("bit");

                    b.Property<decimal>("MaxWeightKg")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Zone")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("CellCode")
                        .IsUnique();

                    b.ToTable("StorageCells");
                });

            modelBuilder.Entity("AISPVZ.Models.SystemSetting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("SettingKey");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)")
                        .HasColumnName("SettingValue");

                    b.HasKey("Id");

                    b.HasIndex("Key")
                        .IsUnique();

                    b.ToTable("SystemSettings");
                });

            modelBuilder.Entity("AISPVZ.Models.IssueOperation", b =>
                {
                    b.HasOne("AISPVZ.Models.Employee", "Employee")
                        .WithMany("IssueOperations")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AISPVZ.Models.Order", "Order")
                        .WithMany("IssueOperations")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AISPVZ.Models.Shift", "Shift")
                        .WithMany("IssueOperations")
                        .HasForeignKey("ShiftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Order");

                    b.Navigation("Shift");
                });

            modelBuilder.Entity("AISPVZ.Models.Order", b =>
                {
                    b.HasOne("AISPVZ.Models.StorageCell", "Cell")
                        .WithMany("Orders")
                        .HasForeignKey("CellId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("AISPVZ.Models.Client", "Client")
                        .WithMany("Orders")
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Cell");

                    b.Navigation("Client");
                });

            modelBuilder.Entity("AISPVZ.Models.OrderItem", b =>
                {
                    b.HasOne("AISPVZ.Models.Order", "Order")
                        .WithMany("Items")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");
                });

            modelBuilder.Entity("AISPVZ.Models.OrderStatusHistory", b =>
                {
                    b.HasOne("AISPVZ.Models.Employee", "Employee")
                        .WithMany("StatusHistories")
                        .HasForeignKey("EmployeeId");

                    b.HasOne("AISPVZ.Models.Order", "Order")
                        .WithMany("StatusHistories")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("AISPVZ.Models.ReturnOperation", b =>
                {
                    b.HasOne("AISPVZ.Models.Employee", "Employee")
                        .WithMany("ReturnOperations")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AISPVZ.Models.Order", "Order")
                        .WithMany("ReturnOperations")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("AISPVZ.Models.Shift", "Shift")
                        .WithMany("ReturnOperations")
                        .HasForeignKey("ShiftId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Order");

                    b.Navigation("Shift");
                });

            modelBuilder.Entity("AISPVZ.Models.Shift", b =>
                {
                    b.HasOne("AISPVZ.Models.Employee", "Employee")
                        .WithMany("Shifts")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("AISPVZ.Models.Client", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("AISPVZ.Models.Employee", b =>
                {
                    b.Navigation("IssueOperations");

                    b.Navigation("ReturnOperations");

                    b.Navigation("Shifts");

                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("AISPVZ.Models.Order", b =>
                {
                    b.Navigation("IssueOperations");

                    b.Navigation("Items");

                    b.Navigation("ReturnOperations");

                    b.Navigation("StatusHistories");
                });

            modelBuilder.Entity("AISPVZ.Models.Shift", b =>
                {
                    b.Navigation("IssueOperations");

                    b.Navigation("ReturnOperations");
                });

            modelBuilder.Entity("AISPVZ.Models.StorageCell", b =>
                {
                    b.Navigation("Orders");
                });
#pragma warning restore 612, 618
        }
    }
}
