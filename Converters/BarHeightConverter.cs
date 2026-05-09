using System.Globalization;
using System.Windows.Data;

namespace Accessory_DesktopApp.Converters;

public sealed class BarHeightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3)
            return 0d;

        var total = ToDecimal(values[0]);
        var max = ToDecimal(values[1]);
        var chartHeight = ToDouble(values[2]);

        if (max <= 0m || chartHeight <= 0d)
            return 0d;

        var ratio = (double)(total / max);
        if (ratio < 0d)
            ratio = 0d;
        if (ratio > 1d)
            ratio = 1d;

        return chartHeight * ratio;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static decimal ToDecimal(object value)
    {
        try
        {
            return value switch
            {
                null => 0m,
                decimal d => d,
                double db => (decimal)db,
                float f => (decimal)f,
                int i => i,
                long l => l,
                string s when decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => System.Convert.ToDecimal(value, CultureInfo.InvariantCulture)
            };
        }
        catch
        {
            return 0m;
        }
    }

    private static double ToDouble(object value)
    {
        try
        {
            return value switch
            {
                null => 0d,
                double d => d,
                float f => f,
                int i => i,
                long l => l,
                decimal m => (double)m,
                string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => System.Convert.ToDouble(value, CultureInfo.InvariantCulture)
            };
        }
        catch
        {
            return 0d;
        }
    }
}
