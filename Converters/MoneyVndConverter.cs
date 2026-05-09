using System.Globalization;
using System.Windows.Data;

namespace Accessory_DesktopApp.Converters;

public sealed class MoneyVndConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
            return "0 ₫";

        decimal amount;

        try
        {
            amount = value switch
            {
                decimal d => d,
                double db => (decimal)db,
                float f => (decimal)f,
                int i => i,
                long l => l,
                string s when decimal.TryParse(s, out var parsed) => parsed,
                _ => System.Convert.ToDecimal(value, culture)
            };
        }
        catch
        {
            return "0 ₫";
        }

        return string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} ₫", amount);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
