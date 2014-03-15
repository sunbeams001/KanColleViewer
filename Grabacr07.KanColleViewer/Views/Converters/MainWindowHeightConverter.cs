using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Grabacr07.KanColleWrapper.Models;

namespace Grabacr07.KanColleViewer.Views.Converters
{
    public class MainWindowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var height = Double.Parse(value.ToString());
            return height + 94; // 26 + 50 + 18
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
