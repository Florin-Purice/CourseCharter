using System;
using System.Globalization;
using System.Windows.Data;

namespace WoTMapWPF
{
    public class PixelsToUnitsConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double pixels;
            double ratio;
            try
            {
                pixels = System.Convert.ToDouble(values[0]);
                ratio = System.Convert.ToDouble(values[1]);
            }
            catch
            {
                pixels = 0;
                ratio = 1;
            }
            return pixels * ratio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
