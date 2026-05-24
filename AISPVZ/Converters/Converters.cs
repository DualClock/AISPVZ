using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using AISPVZ.Models;

namespace AISPVZ.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}

public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Visibility.Collapsed : Visibility.Visible;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}

public class OrderStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Issued => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Green
                OrderStatus.InStorage when IsOverdue(parameter) => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
                OrderStatus.InStorage when IsExpiringToday(parameter) => new SolidColorBrush(Color.FromRgb(255, 152, 0)), // Orange
                OrderStatus.PartialIssued => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
                OrderStatus.Returned => new SolidColorBrush(Color.FromRgb(158, 158, 158)),  // Gray
                _ => new SolidColorBrush(Color.FromRgb(255, 235, 59))  // Yellow
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    private bool IsOverdue(object? parameter)
    {
        if (parameter is Order order)
            return order.PlannedIssueDate.Date < DateTime.Now.Date;
        return false;
    }

    private bool IsExpiringToday(object? parameter)
    {
        if (parameter is Order order)
            return order.PlannedIssueDate.Date == DateTime.Now.Date;
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is OrderStatus status)
        {
            return status switch
            {
                OrderStatus.Pending => "Ожидает",
                OrderStatus.Accepted => "Принят",
                OrderStatus.InStorage => "На хранении",
                OrderStatus.Issued => "Выдан",
                OrderStatus.PartialIssued => "Частично выдан",
                OrderStatus.Returned => "Возвращён",
                OrderStatus.Cancelled => "Отменён",
                _ => status.ToString()
            };
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class MarketplaceToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Marketplace m)
        {
            return m switch
            {
                Marketplace.Ozon => "Ozon",
                Marketplace.Wildberries => "Wildberries",
                Marketplace.YandexMarket => "Яндекс.Маркет",
                Marketplace.Other => "Другое",
                _ => m.ToString()
            };
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CellStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isBusy)
        {
            return isBusy
                ? new SolidColorBrush(Color.FromRgb(244, 67, 54))  
                : new SolidColorBrush(Color.FromRgb(76, 175, 80)); 
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RoleToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Admin => "Администратор",
                EmployeeRole.Operator => "Оператор",
                _ => role.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DecimalToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal d)
            return d.ToString("N2");
        return "0,00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (decimal.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out var result))
            return result;
        return 0m;
    }
}

public class CellStatusToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isBusy)
        {
            return isBusy ? "Занята" : "Свободна";
        }
        return "Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class DoubleToDecimalConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
            return (decimal)d;
        if (value is decimal dc)
            return dc;
        return 0m;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal dc)
            return (double)dc;
        if (value is double d)
            return d;
        return 0.0;
    }
}

public class NullToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return "НОВАЯ ЯЧЕЙКА";
        if (value is int intVal && intVal == 0)
            return "НОВАЯ ЯЧЕЙКА";
        return value.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
