using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AISPVZ.Converters;

public class NullToVisConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value?.ToString()) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InvBoolToVisConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class CellStatusToColorConv : IValueConverter
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

public class CellStatusToBoolConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? "Занята" : "Свободна";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class RoleToTextConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.EmployeeRole role)
        {
            return role switch
            {
                Models.EmployeeRole.Admin => "Администратор",
                Models.EmployeeRole.Operator => "Оператор",
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

public class MarketplaceToTextConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.Marketplace m)
        {
            return m switch
            {
                Models.Marketplace.Ozon => "Ozon",
                Models.Marketplace.Wildberries => "Wildberries",
                Models.Marketplace.YandexMarket => "Яндекс.Маркет",
                Models.Marketplace.Other => "Другое",
                _ => m.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderStatusToTextConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.OrderStatus status)
        {
            return status switch
            {
                Models.OrderStatus.Pending => "Ожидает",
                Models.OrderStatus.Accepted => "Принят",
                Models.OrderStatus.InStorage => "На хранении",
                Models.OrderStatus.Issued => "Выдан",
                Models.OrderStatus.PartialIssued => "Частично выдан",
                Models.OrderStatus.Returned => "Возвращён",
                Models.OrderStatus.Cancelled => "Отменён",
                _ => status.ToString()
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToActiveTextConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? "Активен" : "Неактивен";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStatusColorConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b
            ? new SolidColorBrush(Color.FromRgb(74, 222, 128))
            : new SolidColorBrush(Color.FromRgb(239, 68, 68));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class NullToTextConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value == null ? "Новая запись" : "Редактирование";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class OrderStatusColorConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Models.OrderStatus status)
        {
            return status switch
            {
                Models.OrderStatus.Issued => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                Models.OrderStatus.Returned => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                Models.OrderStatus.PartialIssued => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                _ => new SolidColorBrush(Color.FromRgb(255, 235, 59))
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}