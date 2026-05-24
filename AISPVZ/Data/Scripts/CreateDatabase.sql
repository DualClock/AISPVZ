-- =====================================================
-- AISPVZ Database Creation Script
-- Для старых версий SQL Server (2000/2005)
-- =====================================================

USE master;
GO

-- Проверяем существует ли БД и удаляем если есть
IF EXISTS (SELECT name FROM sysdatabases WHERE name = 'AISPVZ_DB')
BEGIN
    PRINT 'Удаление существующей базы данных...';
    -- Закрываем все соединения
    DECLARE @spid int
    DECLARE cur CURSOR FOR 
        SELECT spid FROM sysprocesses WHERE dbid = DB_ID('AISPVZ_DB')
    OPEN cur
    FETCH NEXT FROM cur INTO @spid
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC ('KILL ' + @spid)
        FETCH NEXT FROM cur INTO @spid
    END
    CLOSE cur
    DEALLOCATE cur
    
    DROP DATABASE AISPVZ_DB;
END
GO

-- Создаем новую базу данных
PRINT 'Создание новой базы данных...';
CREATE DATABASE AISPVZ_DB;
GO

-- Переключаемся на новую БД
USE AISPVZ_DB;
GO

-- =====================================================
-- УДАЛЯЕМ ВСЕ СУЩЕСТВУЮЩИЕ ТАБЛИЦЫ (ЕСЛИ ЕСТЬ)
-- =====================================================
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'OrderStatusHistory' AND type = 'U')
    DROP TABLE OrderStatusHistory;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'ReturnOperations' AND type = 'U')
    DROP TABLE ReturnOperations;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'IssueOperations' AND type = 'U')
    DROP TABLE IssueOperations;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'OrderItems' AND type = 'U')
    DROP TABLE OrderItems;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Shifts' AND type = 'U')
    DROP TABLE Shifts;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Orders' AND type = 'U')
    DROP TABLE Orders;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'StorageCells' AND type = 'U')
    DROP TABLE StorageCells;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Clients' AND type = 'U')
    DROP TABLE Clients;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'Employees' AND type = 'U')
    DROP TABLE Employees;
IF EXISTS (SELECT * FROM sysobjects WHERE name = 'SystemSettings' AND type = 'U')
    DROP TABLE SystemSettings;
GO

PRINT 'Старые таблицы удалены.';
GO

-- =====================================================
-- СОЗДАНИЕ ТАБЛИЦ
-- =====================================================

PRINT 'Создание таблицы Employees...';
CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Login NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

PRINT 'Создание таблицы Clients...';
CREATE TABLE Clients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(50),
    Email NVARCHAR(255),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

PRINT 'Создание таблицы StorageCells...';
CREATE TABLE StorageCells (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CellCode NVARCHAR(50) NOT NULL UNIQUE,
    Zone NVARCHAR(10) NOT NULL DEFAULT 'A',
    IsBusy BIT NOT NULL DEFAULT 0,
    MaxWeightKg FLOAT NOT NULL DEFAULT 30.0,
    Comment NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

PRINT 'Создание таблицы Orders...';
CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    CellId INT NULL,
    Barcode NVARCHAR(100) NOT NULL UNIQUE,
    Marketplace INT NOT NULL DEFAULT 3,
    CurrentStatus INT NOT NULL DEFAULT 0,
    ArrivedAt DATETIME NOT NULL DEFAULT GETDATE(),
    PlannedIssueDate DATETIME NOT NULL,
    IssuedAt DATETIME NULL,
    PhotoPath NVARCHAR(500),
    Comment NVARCHAR(500),
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

PRINT 'Создание таблицы OrderItems...';
CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    Article NVARCHAR(50),
    ProductName NVARCHAR(255) NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    Price FLOAT NOT NULL DEFAULT 0,
    IsIssued BIT NOT NULL DEFAULT 0
);
GO

PRINT 'Создание таблицы Shifts...';
CREATE TABLE Shifts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    StartTime DATETIME NOT NULL DEFAULT GETDATE(),
    EndTime DATETIME NULL,
    IsClosed BIT NOT NULL DEFAULT 0
);
GO

PRINT 'Создание таблицы IssueOperations...';
CREATE TABLE IssueOperations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    EmployeeId INT NOT NULL,
    ShiftId INT NOT NULL,
    IssueDateTime DATETIME NOT NULL DEFAULT GETDATE(),
    Result INT NOT NULL DEFAULT 0,
    TotalAmount FLOAT NOT NULL DEFAULT 0,
    Comment NVARCHAR(500)
);
GO

PRINT 'Создание таблицы ReturnOperations...';
CREATE TABLE ReturnOperations (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    EmployeeId INT NOT NULL,
    ShiftId INT NOT NULL,
    Reason NVARCHAR(500) NOT NULL,
    ReturnDateTime DATETIME NOT NULL DEFAULT GETDATE(),
    ReturnToMarketplaceDate DATETIME NULL
);
GO

PRINT 'Создание таблицы OrderStatusHistory...';
CREATE TABLE OrderStatusHistory (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    OldStatus INT NOT NULL,
    NewStatus INT NOT NULL,
    ChangedAt DATETIME NOT NULL DEFAULT GETDATE(),
    EmployeeId INT NULL
);
GO

PRINT 'Создание таблицы SystemSettings...';
CREATE TABLE SystemSettings (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(100) NOT NULL UNIQUE,
    SettingValue NTEXT NOT NULL,
    Description NVARCHAR(500)
);
GO

-- =====================================================
-- ИСПРАВЛЕНИЕ ТИПОВ ДАННЫХ: FLOAT -> DECIMAL(18,2)
-- Для правильного маппинга на C# decimal
-- =====================================================
PRINT 'Изменение типов данных FLOAT -> DECIMAL...';

ALTER TABLE OrderItems ALTER COLUMN Price DECIMAL(18,2) NOT NULL;
ALTER TABLE IssueOperations ALTER COLUMN TotalAmount DECIMAL(18,2) NOT NULL;
GO

PRINT 'Типы данных исправлены.';
GO

-- =====================================================
-- ДОБАВЛЕНИЕ ВНЕШНИХ КЛЮЧЕЙ
-- =====================================================

PRINT 'Добавление внешних ключей...';

ALTER TABLE Orders ADD CONSTRAINT FK_Orders_Clients FOREIGN KEY (ClientId) REFERENCES Clients(Id) ON DELETE NO ACTION;
ALTER TABLE Orders ADD CONSTRAINT FK_Orders_StorageCells FOREIGN KEY (CellId) REFERENCES StorageCells(Id) ON DELETE SET NULL;
ALTER TABLE OrderItems ADD CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE;
ALTER TABLE Shifts ADD CONSTRAINT FK_Shifts_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE NO ACTION;
ALTER TABLE IssueOperations ADD CONSTRAINT FK_IssueOperations_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE NO ACTION;
ALTER TABLE IssueOperations ADD CONSTRAINT FK_IssueOperations_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE NO ACTION;
ALTER TABLE IssueOperations ADD CONSTRAINT FK_IssueOperations_Shifts FOREIGN KEY (ShiftId) REFERENCES Shifts(Id) ON DELETE NO ACTION;
ALTER TABLE ReturnOperations ADD CONSTRAINT FK_ReturnOperations_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE NO ACTION;
ALTER TABLE ReturnOperations ADD CONSTRAINT FK_ReturnOperations_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE NO ACTION;
ALTER TABLE ReturnOperations ADD CONSTRAINT FK_ReturnOperations_Shifts FOREIGN KEY (ShiftId) REFERENCES Shifts(Id) ON DELETE NO ACTION;
ALTER TABLE OrderStatusHistory ADD CONSTRAINT FK_OrderStatusHistory_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE;
ALTER TABLE OrderStatusHistory ADD CONSTRAINT FK_OrderStatusHistory_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE SET NULL;
GO

-- =====================================================
-- СОЗДАНИЕ ИНДЕКСОВ
-- =====================================================

PRINT 'Создание индексов...';

CREATE INDEX IX_Orders_Barcode ON Orders(Barcode);
CREATE INDEX IX_Orders_ClientId ON Orders(ClientId);
CREATE INDEX IX_Orders_CellId ON Orders(CellId);
CREATE INDEX IX_Orders_CurrentStatus ON Orders(CurrentStatus);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_Shifts_EmployeeId ON Shifts(EmployeeId);
CREATE INDEX IX_IssueOperations_OrderId ON IssueOperations(OrderId);
CREATE INDEX IX_ReturnOperations_OrderId ON ReturnOperations(OrderId);
CREATE INDEX IX_OrderStatusHistory_OrderId ON OrderStatusHistory(OrderId);
GO

-- =====================================================
-- ПРОВЕРКА РЕЗУЛЬТАТА
-- =====================================================
PRINT '==================================================';
PRINT 'База данных AISPVZ_DB успешно создана!';
PRINT '==================================================';
PRINT '';

PRINT 'Список созданных таблиц:';
SELECT name AS 'Table Name' 
FROM sysobjects 
WHERE xtype = 'U' 
ORDER BY name;
PRINT '';

PRINT 'Список внешних ключей:';
SELECT 
    OBJECT_NAME(constid) AS 'Foreign Key Name',
    OBJECT_NAME(fkeyid) AS 'Table Name',
    OBJECT_NAME(rkeyid) AS 'Referenced Table'
FROM sysforeignkeys
ORDER BY OBJECT_NAME(fkeyid);
PRINT '';

PRINT '==================================================';
PRINT 'Готово к работе!';
PRINT '==================================================';
GO