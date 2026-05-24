-- =====================================================
-- ЗАПОЛНЕНИЕ БАЗЫ ДАННЫХ AISPVZ_DB ТЕСТОВЫМИ ДАННЫМИ
-- ДЛЯ MICROSOFT SQL SERVER (ФИНАЛЬНАЯ ВЕРСИЯ)
-- =====================================================

USE AISPVZ_DB;
GO

-- ОЧИСТКА ТАБЛИЦ (удаляем старые данные в правильном порядке)
DELETE FROM OrderStatusHistory;
DELETE FROM ReturnOperations;
DELETE FROM IssueOperations;
DELETE FROM OrderItems;
DELETE FROM Shifts;
DELETE FROM Orders;
DELETE FROM StorageCells;
DELETE FROM Clients;
DELETE FROM Employees;
DELETE FROM SystemSettings;
GO

-- СБРОС AUTOINCREMENT (Identity) для всех таблиц
DBCC CHECKIDENT ('Employees', RESEED, 0);
DBCC CHECKIDENT ('Clients', RESEED, 0);
DBCC CHECKIDENT ('StorageCells', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('Shifts', RESEED, 0);
DBCC CHECKIDENT ('IssueOperations', RESEED, 0);
DBCC CHECKIDENT ('ReturnOperations', RESEED, 0);
DBCC CHECKIDENT ('OrderStatusHistory', RESEED, 0);
DBCC CHECKIDENT ('SystemSettings', RESEED, 0);
GO

-- 1. Заполнение Employees (сотрудники) - 5 записей
INSERT INTO Employees (FullName, Login, PasswordHash, Role, IsActive) VALUES
(N'Иванов Иван Иванович', N'ivanov', N'hash_ivanov123', 0, 1),
(N'Петрова Мария Сергеевна', N'petrova', N'hash_petrova456', 1, 1),
(N'Сидоров Алексей Владимирович', N'sidorov', N'hash_sidorov789', 0, 1),
(N'Козлова Екатерина Андреевна', N'kozlova', N'hash_kozlova321', 2, 1),
(N'Морозов Дмитрий Петрович', N'morozov', N'hash_morozov654', 0, 0);
GO

-- 2. Заполнение Clients (клиенты) - 8 записей
INSERT INTO Clients (FullName, Phone, Email) VALUES
(N'ООО "Рога и Копыта"', N'+7(495)123-45-67', N'info@roga.ru'),
(N'ИП Смирнов А.Б.', N'+7(916)789-12-34', N'smirnov@mail.ru'),
(N'ЗАО "ТехноСервис"', N'+7(812)345-67-89', N'tech@service.com'),
(N'ООО "СтройМаркет"', N'+7(343)567-89-01', N'stroy@market.ru'),
(N'ИП Кузнецов В.В.', N'+7(926)234-56-78', N'kuznetsov@bk.ru'),
(N'ОАО "Торговый Дом"', N'+7(495)987-65-43', N'td@td.ru'),
(N'Сидоренко Андрей Петрович', N'+7(903)111-22-33', N'asidorenko@mail.com'),
(N'ООО "Электроника Плюс"', N'+7(383)222-33-44', N'electron@plus.ru');
GO

-- 3. Заполнение StorageCells (ячейки хранения) - 10 записей
INSERT INTO StorageCells (CellCode, Zone, IsBusy, MaxWeightKg, Comment) VALUES
(N'A-01', N'A', 0, 30.0, N'Маленькая ячейка'),
(N'A-02', N'A', 1, 30.0, N'Средняя ячейка'),
(N'A-03', N'A', 0, 50.0, N'Большая ячейка'),
(N'B-01', N'B', 1, 100.0, N'Для тяжелых грузов'),
(N'B-02', N'B', 0, 100.0, NULL),
(N'B-03', N'B', 1, 150.0, N'VIP ячейка'),
(N'C-01', N'C', 0, 30.0, N'Стеллаж 1'),
(N'C-02', N'C', 1, 30.0, N'Стеллаж 2'),
(N'C-03', N'C', 0, 50.0, NULL),
(N'D-01', N'D', 1, 200.0, N'Для крупногабарита');
GO

-- 4. Заполнение Orders (заказы) - 10 записей
INSERT INTO Orders (ClientId, CellId, Barcode, Marketplace, CurrentStatus, ArrivedAt, PlannedIssueDate, IssuedAt, PhotoPath, Comment) VALUES
(1, 1, N'BARCODE001', 1, 2, DATEADD(day, -10, GETDATE()), DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE()), N'/photos/order1.jpg', N'Срочный заказ'),
(2, 2, N'BARCODE002', 2, 1, DATEADD(day, -8, GETDATE()), DATEADD(day, 2, GETDATE()), NULL, NULL, N'Ожидает выдачи'),
(3, NULL, N'BARCODE003', 3, 0, DATEADD(day, -3, GETDATE()), DATEADD(day, 7, GETDATE()), NULL, NULL, N'Новый заказ'),
(4, 3, N'BARCODE004', 1, 3, DATEADD(day, -15, GETDATE()), DATEADD(day, -10, GETDATE()), DATEADD(day, -10, GETDATE()), N'/photos/order4.jpg', N'Выдан клиенту'),
(5, 4, N'BARCODE005', 2, 4, DATEADD(day, -20, GETDATE()), DATEADD(day, -15, GETDATE()), NULL, N'/photos/order5.jpg', N'Возврат'),
(1, 5, N'BARCODE006', 3, 1, DATEADD(day, -5, GETDATE()), DATEADD(day, 5, GETDATE()), NULL, NULL, N'Ждет в ячейке'),
(6, NULL, N'BARCODE007', 1, 0, DATEADD(day, -2, GETDATE()), DATEADD(day, 8, GETDATE()), NULL, NULL, N'Только поступил'),
(7, 6, N'BARCODE008', 2, 2, DATEADD(day, -12, GETDATE()), DATEADD(day, -8, GETDATE()), DATEADD(day, -8, GETDATE()), N'/photos/order8.jpg', N'Выдан'),
(8, 7, N'BARCODE009', 3, 1, DATEADD(day, -7, GETDATE()), DATEADD(day, 3, GETDATE()), NULL, NULL, N'В обработке'),
(2, 8, N'BARCODE010', 1, 0, DATEADD(day, -1, GETDATE()), DATEADD(day, 9, GETDATE()), NULL, NULL, N'Срочно');
GO

-- 5. Заполнение OrderItems (товары в заказах) - 15 записей
INSERT INTO OrderItems (OrderId, ProductName, Quantity, Price, IsIssued) VALUES
(1, N'Смартфон Xiaomi Note 11', 1, 15000.00, 1),
(1, N'Чехол для смартфона', 2, 500.00, 1),
(2, N'Ноутбук Lenovo IdeaPad', 1, 45000.00, 0),
(3, N'Наушники Sony WH-1000XM4', 1, 25000.00, 0),
(3, N'Зарядное устройство', 1, 1500.00, 0),
(4, N'Клавиатура Logitech', 1, 3500.00, 1),
(4, N'Мышь Logitech', 1, 1200.00, 1),
(5, N'Монитор Samsung 24"', 2, 12000.00, 0),
(6, N'Внешний жесткий диск 1TB', 1, 5000.00, 0),
(6, N'Флешка 64GB', 3, 800.00, 0),
(7, N'Планшет iPad 10.2', 1, 30000.00, 0),
(8, N'Смарт-часы Apple Watch', 1, 28000.00, 1),
(9, N'Фитнес-браслет Xiaomi', 2, 2500.00, 0),
(9, N'Беспроводная зарядка', 1, 1200.00, 0),
(10, N'Роутер TP-Link', 1, 3500.00, 0);
GO

-- 6. Заполнение Shifts (смены) - 6 записей
INSERT INTO Shifts (EmployeeId, StartTime, EndTime, IsClosed) VALUES
(1, DATEADD(hour, -8, GETDATE()), DATEADD(hour, 0, GETDATE()), 1),
(2, DATEADD(day, -1, DATEADD(hour, 9, GETDATE())), DATEADD(day, -1, DATEADD(hour, 18, GETDATE())), 1),
(3, DATEADD(day, -2, DATEADD(hour, 10, GETDATE())), DATEADD(day, -2, DATEADD(hour, 19, GETDATE())), 1),
(1, DATEADD(hour, -4, GETDATE()), NULL, 0),
(4, DATEADD(day, -3, DATEADD(hour, 8, GETDATE())), DATEADD(day, -3, DATEADD(hour, 17, GETDATE())), 1),
(2, DATEADD(hour, -6, GETDATE()), NULL, 0);
GO

-- 7. Заполнение IssueOperations (операции выдачи) - 5 записей
INSERT INTO IssueOperations (OrderId, EmployeeId, ShiftId, IssueDateTime, Result, TotalAmount, Comment) VALUES
(1, 1, 1, DATEADD(day, -5, GETDATE()), 1, 16000.00, N'Выдано полностью'),
(4, 2, 2, DATEADD(day, -10, GETDATE()), 1, 4700.00, N'Выдано без замечаний'),
(8, 3, 3, DATEADD(day, -8, GETDATE()), 1, 28000.00, N'Частичная выдача'),
(7, 1, 4, DATEADD(day, -2, GETDATE()), 0, 0, N'Клиент не пришел'),
(2, 4, 5, DATEADD(day, -6, GETDATE()), 1, 45000.00, N'Выдано успешно');
GO

-- 8. Заполнение ReturnOperations (операции возврата) - 4 записи
INSERT INTO ReturnOperations (OrderId, EmployeeId, ShiftId, Reason, ReturnDateTime, ReturnToMarketplaceDate) VALUES
(5, 2, 2, N'Товар не подошел по характеристикам', DATEADD(day, -18, GETDATE()), DATEADD(day, -15, GETDATE())),
(3, 1, 1, N'Брак в товаре', DATEADD(day, -2, GETDATE()), DATEADD(day, 3, GETDATE())),
(8, 3, 3, N'Передумал покупать', DATEADD(day, -7, GETDATE()), DATEADD(day, -5, GETDATE())),
(6, 4, 5, N'Неверная комплектация', DATEADD(day, -4, GETDATE()), DATEADD(day, 2, GETDATE()));
GO

-- 9. Заполнение OrderStatusHistory (история статусов) - 12 записей
INSERT INTO OrderStatusHistory (OrderId, OldStatus, NewStatus, ChangedAt, EmployeeId) VALUES
(1, 0, 1, DATEADD(day, -9, GETDATE()), 1),
(1, 1, 2, DATEADD(day, -6, GETDATE()), 2),
(2, 0, 1, DATEADD(day, -7, GETDATE()), 3),
(3, 0, 1, DATEADD(day, -2, GETDATE()), 1),
(4, 0, 1, DATEADD(day, -14, GETDATE()), 2),
(4, 1, 2, DATEADD(day, -13, GETDATE()), 3),
(4, 2, 3, DATEADD(day, -11, GETDATE()), 1),
(5, 0, 1, DATEADD(day, -19, GETDATE()), 4),
(5, 1, 2, DATEADD(day, -17, GETDATE()), 2),
(5, 2, 4, DATEADD(day, -16, GETDATE()), 1),
(8, 0, 1, DATEADD(day, -11, GETDATE()), 3),
(8, 1, 2, DATEADD(day, -9, GETDATE()), 2);
GO

-- 10. Заполнение SystemSettings (системные настройки) - 5 записей
INSERT INTO SystemSettings (SettingKey, SettingValue, Description) VALUES
(N'CompanyName', N'АИС ПВЗ', N'Название компании'),
(N'WorkStartTime', N'09:00', N'Начало рабочего дня'),
(N'WorkEndTime', N'21:00', N'Конец рабочего дня'),
(N'MaxOrderWeight', N'30', N'Максимальный вес заказа в кг'),
(N'ReturnDays', N'14', N'Срок возврата товара в днях');
GO

-- =====================================================
-- ПРОВЕРКА ЗАПОЛНЕНИЯ
-- =====================================================
SELECT 'Employees' AS TableName, COUNT(*) AS RecordsCount FROM Employees UNION
SELECT 'Clients', COUNT(*) FROM Clients UNION
SELECT 'StorageCells', COUNT(*) FROM StorageCells UNION
SELECT 'Orders', COUNT(*) FROM Orders UNION
SELECT 'OrderItems', COUNT(*) FROM OrderItems UNION
SELECT 'Shifts', COUNT(*) FROM Shifts UNION
SELECT 'IssueOperations', COUNT(*) FROM IssueOperations UNION
SELECT 'ReturnOperations', COUNT(*) FROM ReturnOperations UNION
SELECT 'OrderStatusHistory', COUNT(*) FROM OrderStatusHistory UNION
SELECT 'SystemSettings', COUNT(*) FROM SystemSettings
ORDER BY TableName;
GO

SELECT TOP 5 Id, FullName, Login FROM Employees;
SELECT TOP 5 Barcode, Comment FROM Orders;
GO