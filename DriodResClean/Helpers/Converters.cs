using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DriodResClean.Helpers
{
	public class BoolToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			bool? val = value as bool?;
			if (val == null)
				return "Open an Android Project";
			return val == true ? "Valid Android Project" : "Invalid Android Project";
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	public class BoolToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			bool? val = value as bool?;
			if (val == null)
				return new SolidColorBrush(Colors.Black);
			return val == true ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
