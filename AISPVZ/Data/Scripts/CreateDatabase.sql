-- =====================================================
-- AISPVZ Database Creation Script
-- Полный скрипт для Microsoft SQL Server
-- =====================================================

-- Создание базы данных
USE master;
GO

-- Проверяем существует ли БД и удаляем если есть
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'AISPVZ_DB')
BEGIN
    ALTER DATABASE AISPVZ_DB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE AISPVZ_DB;
END
GO

-- Создаем новую базу данных
CREATE DATABASE AISPVZ_DB;
GO

-- Переключаемся на новую БД
USE AISPVZ_DB;
GO

-- =====================================================
-- СОЗДАНИЕ ТАБЛИЦ
-- =====================================================

CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Login NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Clients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(50),
    Email NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE StorageCells (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CellCode NVARCHAR(50) NOT NULL UNIQUE,
    Zone NVARCHAR(10) NOT NULL DEFAULT 'A',
    IsBusy BIT NOT NULL DEFAULT 0,
    MaxWeightKg FLOAT NOT NULL DEFAULT 30.0,
    Comment NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    CellId INT NULL,
    Barcode NVARCHAR(100) NOT NULL UNIQUE,
    Marketplace INT NOT NULL DEFAULT 3,
    CurrentStatus INT NOT NULL DEFAULT 0,
    ArrivedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    PlannedIssueDate DATETIME2 NOT NULL,
    IssuedAt DATETIME2 NULL,
    PhotoPath NVARCHAR(500),
    Comment NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (ClientId) REFERENCES Clients(Id),
    FOREIGN KEY (CellId) REFERENCES StorageCells(Id)
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    Article NVARCHAR(50),
    ProductName NVARCHAR(255) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    Price FLOAT NOT NULL DEFAULT 0,
    IsIssued BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);

CREATE TABLE Shifts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    EndTime DATETIME2 NULL,
    IsClosed BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE IssueOperations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    EmployeeId INT NOT NULL,
    ShiftId INT NOT NULL,
    IssueDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    Result INT NOT NULL DEFAULT 0,
    TotalAmount FLOAT NOT NULL DEFAULT 0,
    Comment NVARCHAR(500),
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id)
);

CREATE TABLE ReturnOperations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    EmployeeId INT NOT NULL,
    ShiftId INT NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    ReturnDateTime DATETIME2 NOT NULL DEFAULT GETDATE(),
    ReturnToMarketplaceDate DATETIME2 NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id),
    FOREIGN KEY (ShiftId) REFERENCES Shifts(Id)
);

CREATE TABLE OrderStatusHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    OldStatus INT NOT NULL,
    NewStatus INT NOT NULL,
    ChangedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    EmployeeId INT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
);

CREATE TABLE SystemSettings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500)
);

-- =====================================================
-- СОЗДАНИЕ ИНДЕКСОВ
-- =====================================================

CREATE INDEX IX_Orders_Barcode ON Orders(Barcode);
CREATE INDEX IX_Orders_ClientId ON Orders(ClientId);
CREATE INDEX IX_Orders_CellId ON Orders(CellId);
CREATE INDEX IX_Orders_CurrentStatus ON Orders(CurrentStatus);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_Shifts_EmployeeId ON Shifts(EmployeeId);
CREATE INDEX IX_IssueOperations_OrderId ON IssueOperations(OrderId);
CREATE INDEX IX_ReturnOperations_OrderId ON ReturnOperations(OrderId);
CREATE INDEX IX_OrderStatusHistory_OrderId ON OrderStatusHistory(OrderId);

-- =====================================================
-- ВЫВОД ИНФОРМАЦИИ О СОЗДАНИИ
-- =====================================================
PRINT 'База данных AISPVZ_DB успешно создана!';
PRINT 'Таблицы и индексы созданы.';
GO

-- Показываем список всех таблиц
SELECT 
    TABLE_NAME AS 'Таблица',
    TABLE_TYPE AS 'Тип'
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;