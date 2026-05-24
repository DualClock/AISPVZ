# =====================================================
# AISPVZ — Скрипт быстрой установки
# Запуск: powershell -ExecutionPolicy Bypass -File setup.ps1
# =====================================================

param(
    [string]$SqlServer = "localhost",
    [string]$DatabaseName = "AISPVZ_DB"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$ProjectDir = Join-Path $ProjectRoot "AISPVZ"

Write-Host "==================================================" -ForegroundColor Cyan
Write-Host " AISPVZ — Быстрая установка" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# 1. Проверка .NET SDK
Write-Host "`n[1/5] Проверка .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: .NET SDK не найден. Установите .NET 9 SDK: https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Red
    exit 1
}
if ($dotnetVersion -notmatch "^9\.") {
    Write-Host "ВНИМАНИЕ: Рекомендуется .NET 9. Текущая версия: $dotnetVersion" -ForegroundColor Yellow
}
Write-Host "OK: .NET $dotnetVersion" -ForegroundColor Green

# 2. Восстановление NuGet пакетов
Write-Host "`n[2/5] Восстановление NuGet пакетов..." -ForegroundColor Yellow
Set-Location $ProjectDir
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: Не удалось восстановить пакеты" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Пакеты восстановлены" -ForegroundColor Green

# 3. Проверка SQL Server
Write-Host "`n[3/5] Проверка SQL Server..." -ForegroundColor Yellow
$sqlCheck = sqlcmd -S $SqlServer -E -Q "SELECT @@VERSION" -b 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ВНИМАНИЕ: Не удалось подключиться к SQL Server '$SqlServer'. Пропуск создания БД." -ForegroundColor Yellow
    Write-Host "Вы можете создать базу данных вручную, выполнив скрипты из папки Data\Scripts\" -ForegroundColor Yellow
    $createDb = $false
} else {
    Write-Host "OK: SQL Server доступен" -ForegroundColor Green
    $createDb = $true
}

# 4. Создание базы данных
if ($createDb) {
    Write-Host "`n[4/5] Создание базы данных..." -ForegroundColor Yellow

    $createScript = Join-Path $ProjectRoot "AISPVZ\Data\Scripts\CreateDatabase.sql"
    $seedScript = Join-Path $ProjectRoot "AISPVZ\Data\Scripts\SeedData.sql"

    if (-not (Test-Path $createScript)) {
        Write-Host "ОШИБКА: Скрипт CreateDatabase.sql не найден" -ForegroundColor Red
        exit 1
    }

    Write-Host " Выполнение CreateDatabase.sql..." -ForegroundColor Gray
    sqlcmd -S $SqlServer -E -i $createScript -b
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ВНИМАНИЕ: Ошибка при создании БД. Возможно, база уже существует." -ForegroundColor Yellow
    } else {
        Write-Host " OK: База данных создана" -ForegroundColor Green
    }

    Write-Host " Выполнение SeedData.sql..." -ForegroundColor Gray
    if (Test-Path $seedScript) {
        sqlcmd -S $SqlServer -E -i $seedScript -b
        if ($LASTEXITCODE -ne 0) {
            Write-Host "ВНИМАНИЕ: Ошибка при заполнении БД тестовыми данными" -ForegroundColor Yellow
        } else {
            Write-Host " OK: Тестовые данные загружены" -ForegroundColor Green
        }
    }
}

# 5. Сборка проекта
Write-Host "`n[5/5] Сборка проекта..." -ForegroundColor Yellow
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "ОШИБКА: Сборка не удалась" -ForegroundColor Red
    exit 1
}
Write-Host "OK: Проект собран успешно" -ForegroundColor Green

Write-Host "`n==================================================" -ForegroundColor Cyan
Write-Host " Установка завершена!" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Запуск приложения:" -ForegroundColor White
Write-Host "  cd $ProjectDir" -ForegroundColor Gray
Write-Host "  dotnet run" -ForegroundColor Gray
Write-Host ""
Write-Host "Тестовые учётные записи:" -ForegroundColor White
Write-Host "  Логин: ivanov" -ForegroundColor Gray
Write-Host "  Пароль: password123" -ForegroundColor Gray
Write-Host "==================================================" -ForegroundColor Cyan
