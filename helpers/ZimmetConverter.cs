using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfApp1.Helpers
{
    public class ZimmetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var ad = values != null && values.Length > 0 ? values[0] as string : null;
            var departman = values != null && values.Length > 1 ? values[1] as string : null;

            if (string.IsNullOrWhiteSpace(ad))
                return "Zimmetli değil";

            return string.IsNullOrWhiteSpace(departman)
                ? "Zimmetli: " + ad
                : "Zimmetli: " + ad + " (" + departman + ")";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
