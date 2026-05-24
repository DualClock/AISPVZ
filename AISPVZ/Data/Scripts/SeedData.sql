-- =====================================================
-- AISPVZ Database Creation Script
-- С поддержкой русского языка и правильными типами данных
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

-- Создаем новую базу данных с поддержкой русского языка
PRINT 'Создание новой базы данных с поддержкой русского языка...';
CREATE DATABASE AISPVZ_DB
COLLATE Cyrillic_General_CI_AS;
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
    MaxWeightKg DECIMAL(18,2) NOT NULL DEFAULT 30.0,
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
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
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
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
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
-- ТИПЫ ДАННЫХ УЖЕ ПРАВИЛЬНЫЕ (DECIMAL), НО НА ВСЯКИЙ СЛУЧАЙ ПРОВЕРИМ
-- =====================================================
PRINT 'Проверка типов данных...';
PRINT 'Типы данных уже установлены в DECIMAL(18,2), конвертация не требуется.';
GO

-- =====================================================
-- ЗАПОЛНЕНИЕ БАЗОВЫМИ ДАННЫМИ
-- =====================================================

PRINT '==================================================';
PRINT 'Заполнение базовыми данными...';
PRINT '==================================================';
GO

-- 1. Заполнение Employees
PRINT 'Заполнение Employees...';

IF NOT EXISTS (SELECT 1 FROM Employees WHERE Login = N'ivanov')
BEGIN
    INSERT INTO Employees (FullName, Login, PasswordHash, Role, IsActive, CreatedAt)
    VALUES 
        (N'Иванов Иван Иванович', N'ivanov', N'password123', 0, 1, GETDATE()),
        (N'Петрова Мария Сергеевна', N'petrova', N'password456', 1, 1, GETDATE()),
        (N'Сидоров Алексей Владимирович', N'sidorov', N'password789', 2, 1, GETDATE()),
        (N'Козлова Елена Дмитриевна', N'kozlova', N'password321', 2, 0, GETDATE()),
        (N'Морозов Дмитрий Петрович', N'morozov', N'password654', 2, 0, GETDATE());
    PRINT N'  Добавлено 5 сотрудников.';
END
ELSE
BEGIN
    PRINT N'  Сотрудники уже существуют, пропуск.';
END
GO

-- 2. Заполнение Clients
PRINT 'Заполнение Clients...';

IF NOT EXISTS (SELECT 1 FROM Clients WHERE Phone = N'+7-495-111-2233')
BEGIN
    INSERT INTO Clients (FullName, Phone, Email, CreatedAt)
    VALUES 
        (N'ООО "Ромашка"', N'+7-495-111-2233', N'info@romashka.ru', GETDATE()),
        (N'ИП Смирнов А.Н.', N'+7-495-222-3344', N'smirnov@mail.ru', GETDATE()),
        (N'ЗАО "ТехноПарк"', N'+7-495-333-4455', N'tech@technopark.ru', GETDATE()),
        (N'ООО "МегаСтрой"', N'+7-495-444-5566', N'megastroy@yandex.ru', GETDATE()),
        (N'ИП Васильева Е.В.', N'+7-495-555-6677', N'vasilyeva@gmail.com', GETDATE()),
        (N'ОАО "ПромКомплект"', N'+7-495-666-7788', N'prom@promkomplekt.ru', GETDATE()),
        (N'ООО "Эко-Трейд"', N'+7-495-777-8899', N'eco@ecotrade.ru', GETDATE());
    PRINT N'  Добавлено 7 клиентов.';
END
ELSE
BEGIN
    PRINT N'  Клиенты уже существуют, пропуск.';
END
GO

-- 3. Заполнение StorageCells
PRINT 'Заполнение StorageCells...';

IF NOT EXISTS (SELECT 1 FROM StorageCells WHERE CellCode = N'A-01')
BEGIN
    INSERT INTO StorageCells (CellCode, Zone, IsBusy, MaxWeightKg, Comment, CreatedAt)
    VALUES 
        (N'A-01', N'A', 0, 30.00, N'Стандартная ячейка', GETDATE()),
        (N'A-02', N'A', 0, 30.00, N'Стандартная ячейка', GETDATE()),
        (N'A-03', N'A', 1, 50.00, N'Усиленная ячейка', GETDATE()),
        (N'A-04', N'A', 0, 30.00, NULL, GETDATE()),
        (N'B-01', N'B', 1, 100.00, N'Для крупногабарита', GETDATE()),
        (N'B-02', N'B', 0, 100.00, N'Для крупногабарита', GETDATE()),
        (N'B-03', N'B', 0, 50.00, NULL, GETDATE()),
        (N'C-01', N'C', 0, 30.00, N'Стандартная ячейка', GETDATE()),
        (N'C-02', N'C', 1, 30.00, NULL, GETDATE()),
        (N'C-03', N'C', 0, 50.00, N'Стеллаж', GETDATE());
    PRINT N'  Добавлено 10 ячеек.';
END
ELSE
BEGIN
    PRINT N'  Ячейки уже существуют, пропуск.';
END
GO

-- 4. Заполнение Orders
PRINT 'Заполнение Orders...';

IF NOT EXISTS (SELECT 1 FROM Orders WHERE Barcode = N'ORD-2025001')
BEGIN
    INSERT INTO Orders (ClientId, CellId, Barcode, Marketplace, CurrentStatus, ArrivedAt, PlannedIssueDate, IssuedAt, Comment, CreatedAt)
    VALUES 
        (1, 1, N'ORD-2025001', 1, 2, DATEADD(day, -5, GETDATE()), DATEADD(day, 2, GETDATE()), NULL, N'Срочный заказ', GETDATE()),
        (2, 2, N'ORD-2025002', 2, 2, DATEADD(day, -3, GETDATE()), DATEADD(day, 4, GETDATE()), NULL, NULL, GETDATE()),
        (3, 3, N'ORD-2025003', 3, 3, DATEADD(day, -7, GETDATE()), DATEADD(day, -1, GETDATE()), DATEADD(day, -2, GETDATE()), N'Выдан клиенту', GETDATE()),
        (4, 4, N'ORD-2025004', 1, 1, DATEADD(day, -1, GETDATE()), DATEADD(day, 6, GETDATE()), NULL, N'Ожидает размещения', GETDATE()),
        (5, 5, N'ORD-2025005', 2, 2, DATEADD(day, -4, GETDATE()), DATEADD(day, 3, GETDATE()), NULL, NULL, GETDATE()),
        (6, NULL, N'ORD-2025006', 3, 0, DATEADD(day, -2, GETDATE()), DATEADD(day, 5, GETDATE()), NULL, N'Без ячейки', GETDATE()),
        (7, 6, N'ORD-2025007', 1, 2, DATEADD(day, -6, GETDATE()), DATEADD(day, 1, GETDATE()), NULL, N'Требуется проверка', GETDATE()),
        (2, NULL, N'ORD-2025008', 2, 0, DATEADD(day, -1, GETDATE()), DATEADD(day, 7, GETDATE()), NULL, N'Новый заказ', GETDATE()),
        (3, 7, N'ORD-2025009', 1, 3, DATEADD(day, -8, GETDATE()), DATEADD(day, -3, GETDATE()), DATEADD(day, -4, GETDATE()), N'Выдан', GETDATE()),
        (1, 8, N'ORD-2025010', 3, 1, DATEADD(day, -2, GETDATE()), DATEADD(day, 5, GETDATE()), NULL, N'На проверке', GETDATE());
    PRINT N'  Добавлено 10 заказов.';
END
ELSE
BEGIN
    PRINT N'  Заказы уже существуют, пропуск.';
END
GO

-- 5. Заполнение OrderItems
PRINT 'Заполнение OrderItems...';

IF NOT EXISTS (SELECT 1 FROM OrderItems WHERE Article = N'ART-001')
BEGIN
    INSERT INTO OrderItems (OrderId, Article, ProductName, Quantity, Price, IsIssued)
    VALUES 
        (1, N'ART-001', N'Смартфон Samsung Galaxy A54', 2, 25000.00, 0),
        (1, N'ART-002', N'Чехол для телефона', 2, 500.00, 0),
        (2, N'ART-003', N'Ноутбук Lenovo ThinkPad', 1, 55000.00, 0),
        (2, N'ART-004', N'Мышь беспроводная Logitech', 1, 1500.00, 0),
        (3, N'ART-005', N'Наушники Sony WH-1000XM5', 1, 30000.00, 1),
        (3, N'ART-006', N'Зарядное устройство', 2, 800.00, 1),
        (4, N'ART-007', N'Планшет iPad 10.2', 1, 35000.00, 0),
        (5, N'ART-008', N'Фитнес-браслет Xiaomi', 3, 3000.00, 0),
        (5, N'ART-009', N'Блок питания 20W', 3, 600.00, 0),
        (6, N'ART-010', N'Телевизор LG 55"', 1, 45000.00, 0),
        (7, N'ART-011', N'Кофемашина DeLonghi', 1, 35000.00, 0),
        (7, N'ART-012', N'Таблетки для кофемашины', 10, 200.00, 0),
        (8, N'ART-013', N'Робот-пылесос', 1, 25000.00, 0),
        (9, N'ART-014', N'Электросамокат', 1, 40000.00, 1),
        (9, N'ART-015', N'Шлем защитный', 1, 2000.00, 1),
        (10, N'ART-016', N'Умные часы Apple Watch', 2, 30000.00, 0),
        (10, N'ART-017', N'Ремешок для часов', 2, 1000.00, 0);
    PRINT N'  Добавлено 17 товарных позиций.';
END
ELSE
BEGIN
    PRINT N'  Товары уже существуют, пропуск.';
END
GO

-- 6. Заполнение Shifts
PRINT 'Заполнение Shifts...';

IF NOT EXISTS (SELECT 1 FROM Shifts WHERE EmployeeId = 1 AND IsClosed = 1)
BEGIN
    INSERT INTO Shifts (EmployeeId, StartTime, EndTime, IsClosed)
    VALUES 
        (1, DATEADD(hour, -8, GETDATE()), DATEADD(hour, 0, GETDATE()), 1),
        (2, DATEADD(hour, -16, GETDATE()), DATEADD(hour, -8, GETDATE()), 1),
        (1, DATEADD(hour, -24, GETDATE()), DATEADD(hour, -16, GETDATE()), 1),
        (3, DATEADD(hour, -8, GETDATE()), NULL, 0),
        (2, DATEADD(hour, -32, GETDATE()), DATEADD(hour, -24, GETDATE()), 1);
    PRINT N'  Добавлено 5 смен.';
END
ELSE
BEGIN
    PRINT N'  Смены уже существуют, пропуск.';
END
GO

-- 7. Заполнение IssueOperations
PRINT 'Заполнение IssueOperations...';

IF NOT EXISTS (SELECT 1 FROM IssueOperations WHERE OrderId = 3 AND Result = 1)
BEGIN
    INSERT INTO IssueOperations (OrderId, EmployeeId, ShiftId, IssueDateTime, Result, TotalAmount, Comment)
    VALUES 
        (3, 1, 1, DATEADD(day, -2, GETDATE()), 1, 30800.00, N'Выдано полностью'),
        (9, 2, 2, DATEADD(day, -4, GETDATE()), 1, 42000.00, N'Выдано клиенту'),
        (1, 3, 4, GETDATE(), 0, 0.00, N'Частичная выдача (в процессе)');
    PRINT N'  Добавлено 3 операции выдачи.';
END
ELSE
BEGIN
    PRINT N'  Операции выдачи уже существуют, пропуск.';
END
GO

-- 8. Заполнение ReturnOperations
PRINT 'Заполнение ReturnOperations...';

IF NOT EXISTS (SELECT 1 FROM ReturnOperations WHERE OrderId = 1 AND Reason = N'Брак товара')
BEGIN
    INSERT INTO ReturnOperations (OrderId, EmployeeId, ShiftId, Reason, ReturnDateTime, ReturnToMarketplaceDate)
    VALUES 
        (1, 1, 1, N'Брак товара', DATEADD(day, -3, GETDATE()), DATEADD(day, 5, GETDATE())),
        (5, 2, 2, N'Не подошел по размеру', DATEADD(day, -2, GETDATE()), DATEADD(day, 7, GETDATE()));
    PRINT N'  Добавлено 2 операции возврата.';
END
ELSE
BEGIN
    PRINT N'  Операции возврата уже существуют, пропуск.';
END
GO

-- 9. Заполнение OrderStatusHistory
PRINT 'Заполнение OrderStatusHistory...';

IF NOT EXISTS (SELECT 1 FROM OrderStatusHistory WHERE OrderId = 1 AND NewStatus = 2)
BEGIN
    INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, EmployeeId)
    VALUES 
        (1, 0, 1, DATEADD(day, -5, GETDATE()), 1),
        (1, 1, 2, DATEADD(day, -4, GETDATE()), 1),
        (2, 0, 1, DATEADD(day, -3, GETDATE()), 2),
        (2, 1, 2, DATEADD(day, -2, GETDATE()), 2),
        (3, 0, 1, DATEADD(day, -7, GETDATE()), 1),
        (3, 1, 2, DATEADD(day, -6, GETDATE()), 1),
        (3, 2, 3, DATEADD(day, -5, GETDATE()), 1),
        (4, 0, 1, DATEADD(day, -1, GETDATE()), 3),
        (5, 0, 1, DATEADD(day, -4, GETDATE()), 2),
        (5, 1, 2, DATEADD(day, -3, GETDATE()), 2),
        (7, 0, 1, DATEADD(day, -6, GETDATE()), 1),
        (7, 1, 2, DATEADD(day, -5, GETDATE()), 1),
        (9, 0, 1, DATEADD(day, -8, GETDATE()), 2),
        (9, 1, 2, DATEADD(day, -7, GETDATE()), 2),
        (9, 2, 3, DATEADD(day, -6, GETDATE()), 2);
    PRINT N'  Добавлено 15 записей истории.';
END
ELSE
BEGIN
    PRINT N'  История статусов уже существует, пропуск.';
END
GO

-- 10. Заполнение SystemSettings
PRINT 'Заполнение SystemSettings...';

IF NOT EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = N'CompanyName')
BEGIN
    INSERT INTO SystemSettings (SettingKey, SettingValue, Description)
    VALUES 
        (N'CompanyName', N'{"value":"ООО АИС ПВЗ"}', N'Название компании'),
        (N'StorageLimit', N'{"value":100}', N'Максимальное количество заказов на складе'),
        (N'DefaultReturnDays', N'{"value":14}', N'Срок возврата по умолчанию (дней)'),
        (N'WorkingHours', N'{"start":"09:00","end":"21:00"}', N'Часы работы ПВЗ'),
        (N'NotificationEmail', N'{"email":"support@aispvz.ru"}', N'Email для уведомлений');
    PRINT N'  Добавлено 5 настроек.';
END
ELSE
BEGIN
    PRINT N'  Настройки уже существуют, пропуск.';
END
GO

-- =====================================================
-- ОБНОВЛЕНИЕ СТАТУСА ЯЧЕЕК (IsBusy)
-- =====================================================
PRINT 'Обновление статуса ячеек...';

UPDATE StorageCells 
SET IsBusy = 1 
WHERE Id IN (SELECT DISTINCT CellId FROM Orders WHERE CellId IS NOT NULL AND IssuedAt IS NULL);

PRINT N'  Статус ячеек обновлен.';
GO

-- =====================================================
-- ВЫВОД СТАТИСТИКИ
-- =====================================================
PRINT '==================================================';
PRINT 'СТАТИСТИКА ЗАПОЛНЕНИЯ:';
PRINT '==================================================';

DECLARE @empCount INT, @clientCount INT, @cellCount INT, @orderCount INT, @itemCount INT;
DECLARE @shiftCount INT, @issueCount INT, @returnCount INT, @historyCount INT, @settingCount INT;

SELECT @empCount = COUNT(*) FROM Employees;
SELECT @clientCount = COUNT(*) FROM Clients;
SELECT @cellCount = COUNT(*) FROM StorageCells;
SELECT @orderCount = COUNT(*) FROM Orders;
SELECT @itemCount = COUNT(*) FROM OrderItems;
SELECT @shiftCount = COUNT(*) FROM Shifts;
SELECT @issueCount = COUNT(*) FROM IssueOperations;
SELECT @returnCount = COUNT(*) FROM ReturnOperations;
SELECT @historyCount = COUNT(*) FROM OrderStatusHistory;
SELECT @settingCount = COUNT(*) FROM SystemSettings;

PRINT N'Employees: ' + CAST(@empCount AS NVARCHAR(10));
PRINT N'Clients: ' + CAST(@clientCount AS NVARCHAR(10));
PRINT N'StorageCells: ' + CAST(@cellCount AS NVARCHAR(10));
PRINT N'Orders: ' + CAST(@orderCount AS NVARCHAR(10));
PRINT N'OrderItems: ' + CAST(@itemCount AS NVARCHAR(10));
PRINT N'Shifts: ' + CAST(@shiftCount AS NVARCHAR(10));
PRINT N'IssueOperations: ' + CAST(@issueCount AS NVARCHAR(10));
PRINT N'ReturnOperations: ' + CAST(@returnCount AS NVARCHAR(10));
PRINT N'OrderStatusHistory: ' + CAST(@historyCount AS NVARCHAR(10));
PRINT N'SystemSettings: ' + CAST(@settingCount AS NVARCHAR(10));
GO

PRINT '==================================================';
PRINT 'База данных успешно создана и заполнена!';
PRINT '==================================================';
PRINT '';
PRINT N'Данные для входа в систему:';
PRINT N'  Логин: ivanov, Пароль: password123 (Администратор)';
PRINT N'  Логин: petrova, Пароль: password456 (Менеджер)';
PRINT N'  Логин: sidorov, Пароль: password789 (Оператор)';
PRINT N'  Логин: kozlova, Пароль: password321 (Оператор, неактивен)';
PRINT N'  Логин: morozov, Пароль: password654 (Оператор, неактивен)';
PRINT '==================================================';
GO 
