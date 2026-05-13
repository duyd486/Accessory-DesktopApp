using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Accessory_DesktopApp.Converters
{
    public class NoCacheImageConverter : IValueConverter
    {
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (value == null)
                return null;

            var url = value.ToString();

            if (string.IsNullOrEmpty(url))
                return null;

            // thêm random query
            url += (url.Contains("?") ? "&" : "?")
                + "t=" + DateTime.Now.Ticks;

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(url, UriKind.Absolute);
            bitmap.EndInit();

            //bitmap.Freeze();

            return bitmap;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
