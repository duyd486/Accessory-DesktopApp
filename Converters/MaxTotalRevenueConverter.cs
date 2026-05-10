using Accessory_DesktopApp.Models.Dtos;
using System.Globalization;
using System.Windows.Data;

namespace Accessory_DesktopApp.Converters;

public sealed class MaxTotalRevenueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not IEnumerable<MonthlyRevenueDto> items)
            return 0m;

        decimal max = 0m;
        foreach (var item in items)
        {
            if (item is null)
                continue;

            if (item.Total > max)
                max = item.Total;
        }

        return max;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
