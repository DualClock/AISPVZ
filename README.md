# AISPVZ — Автоматизированная информационная система «Пункт выдачи заказов»

Десктопное WPF-приложение для управления пунктом выдачи заказов. Реализовано на .NET 9 с использованием паттерна MVVM, Entity Framework Core и SQL Server.

![.NET 9](https://img.shields.io/badge/.NET-9.0-blue)
![WPF](https://img.shields.io/badge/WPF-UI-orange)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-red)

---

## Возможности

- **Управление заказами** — приём, хранение и выдача заказов
- **Учёт клиентов** — хранение контактных данных
- **Складирование** — управление ячейками хранения по зонам
- **Смены** — учёт рабочих смен сотрудников
- **Авторизация** — авторизация по логину и паролю с хешированием BCrypt
- **QR-коды** — генерация и сканирование QR для идентификации заказов
- **Отчётность** — экспорт данных в Excel и CSV
- **Печать** — поддержка печати этикеток и документов
- **Material Design** — современный пользовательский интерфейс

---

## Требования

| Компонент | Минимум |
|-----------|---------|
| Windows | 10/11 |
| .NET SDK | 9.0 |
| SQL Server | 2019 и выше |
| Visual Studio | 2022 (опционально) |

---

## Установка

### 1. Клонирование репозитория

```bash
git clone https://github.com/DualClock/AISPVZ.git
cd AISPVZ
```

### 2. Установка .NET SDK

Скачайте и установите [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

Проверьте установку:

```bash
dotnet --version
# 9.0.x
```

### 3. Настройка SQL Server

Убедитесь, что SQL Server запущен и доступен. По умолчанию приложение ожидает подключение к `localhost` с аутентификацией Windows.

---

## Настройка базы данных

### Опция 1: Через SQL Server Management Studio (SSMS)

1. Откройте SSMS и подключитесь к серверу
2. Откройте файл `AISPVZ/Data/Scripts/CreateDatabase.sql`
3. Выполните скрипт (F5 или Execute)
4. Затем откройте и выполните `AISPVZ/Data/Scripts/SeedData.sql` для заполнения тестовыми данными

### Опция 2: Через командную строку

```bash
sqlcmd -S localhost -E -i AISPVZ/Data/Scripts/CreateDatabase.sql
sqlcmd -S localhost -E -i AISPVZ/Data/Scripts/SeedData.sql
```

### Структура базы данных

Приложение использует следующие таблицы:

| Таблица | Назначение |
|---------|-----------|
| `Employees` | Сотрудники (с хешированными паролями) |
| `Clients` | Клиенты |
| `StorageCells` | Ячейки хранения (зоны A, B, C, D) |
| `Orders` | Заказы со статусами |
| `OrderItems` | Товары внутри заказов |
| `Shifts` | Рабочие смены |
| `IssueOperations` | Операции выдачи |
| `ReturnOperations` | Операции возврата |
| `OrderStatusHistory` | История смены статусов |
| `SystemSettings` | Системные настройки |

### Тестовые учётные записи

После выполнения `SeedData.sql` доступны сотрудники:

| Логин | Пароль | Роль |
|-------|--------|------|
| ivanov | password123 | Администратор |
| petrova | password456 | Менеджер |
| sidorov | password789 | Оператор |
| kozlova | password321 | Оператор |
| morozov | password654 | Оператор (неактивен) |

> **Важно:** Реальные пароли хешируются через BCrypt. Тестовые хеши в SeedData.sql — демонстрационные. Для реального развёртывания используйте функцию хеширования или измените хеши вручную через приложение.

---

## Сборка и запуск

### Через командную строку

```bash
cd AISPVZ
dotnet restore
dotnet build
dotnet run
```

### Через Visual Studio

1. Откройте `AISPVZ.slnx` или `AISPVZ/AISPVZ.csproj`
2. Нажмите `F5` для запуска

### Публикация

```bash
dotnet publish -c Release -o ./publish
```

---

## Структура проекта

```
AISPVZ/
├── Data/
│   ├── Context/          # EF Core DbContext
│   ├── Migrations/       # EF Core миграции
│   ├── Scripts/          # SQL-скрипты для БД
│   └── DbInitializer.cs  # Инициализатор БД
├── Models/               # Модели данных (Employee, Order, Client...)
├── ViewModels/           # MVVM ViewModels
├── Views/                # XAML-окна и страницы
├── Services/             # Бизнес-логика
│   ├── AuthService.cs    # Авторизация
│   ├── DatabaseService.cs
│   ├── ExportService.cs  # Экспорт в Excel/CSV
│   ├── OrderService.cs
│   ├── PrintService.cs
│   ├── QrCodeService.cs
│   ├── ReportService.cs
│   ├── ShiftService.cs
│   └── ReferenceService.cs
├── Converters/          # Конвертеры XAML
├── Helpers/              # Поведения и утилиты
└── Resources/            # Темы и ресурсы XAML
```

---

## Используемые NuGet-пакеты

| Пакет | Версия | Назначение |
|-------|--------|------------|
| `CommunityToolkit.Mvvm` | 8.2.2 | MVVM-инфраструктура |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.0.0 | ORM для SQL Server |
| `Microsoft.Extensions.DependencyInjection` | 9.0.0 | DI-контейнер |
| `BCrypt.Net-Next` | 4.0.3 | Хеширование паролей |
| `QRCoder` | 1.6.0 | Генерация QR-кодов |
| `ClosedXML` | 0.104.1 | Экспорт в Excel |
| `CsvHelper` | 33.0.1 | Экспорт в CSV |
| `MaterialDesignThemes` | 5.1.0 | UI-фреймворк |

---

## Статусы заказов

| Код | Статус | Описание |
|-----|--------|---------|
| 0 | Новый | Заказ только поступил |
| 1 | В обработке | Принят и размещён |
| 2 | Готов к выдаче | Ожидает клиента |
| 3 | Выдан | Полностью выдан |
| 4 | Возврат | Возвращён на склад |

---

## Маркетплейсы

| Код | Название |
|-----|----------|
| 0 | Другое |
| 1 | Wildberries |
| 2 | Ozon |
| 3 | Яндекс Маркет |

---

## Лицензия

MIT
