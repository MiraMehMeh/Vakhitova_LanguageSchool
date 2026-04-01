using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Data;

namespace Vakhitova_LanguageSchool
{
    internal class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return GetDefaultImage();
            string path = value.ToString();
            if (string.IsNullOrEmpty(path)) return GetDefaultImage();

            path = path.Replace('\\', '/');
            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            if (File.Exists(fullPath))
                return new BitmapImage(new Uri(fullPath));
            else
                return GetDefaultImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapImage GetDefaultImage()
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Клиенты", "keypad.jpg");
            if (File.Exists(defaultPath))
                return new BitmapImage(new Uri(defaultPath));
            return null;
        }
    }
}
