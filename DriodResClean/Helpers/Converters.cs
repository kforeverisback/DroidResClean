using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DriodResClean.Helper
{
	public class StatusToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			ViewModel.StatusEnum? val = value as ViewModel.StatusEnum?;

			string retVal = "";
			if (val == null)
				return retVal;

			switch(val)
			{
				case ViewModel.StatusEnum.EmptyProjectPath:
					retVal = "Open Android Project";
					break;
				case ViewModel.StatusEnum.InvalidDroidProject:
					retVal = "Invalid Android Project";
					break;
				case ViewModel.StatusEnum.ProcessSuccess:
					retVal = "Process Success!";
					break;
				case ViewModel.StatusEnum.ValidDroidProject:
					retVal = "Valid Android Project";
					break;
				case ViewModel.StatusEnum.Processing:
					retVal = "Processing. Please Wait...";
					break;
				default:
					break;
			}
			return retVal;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}

	public class StatusToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			SolidColorBrush sb = new SolidColorBrush(Colors.Black);
			ViewModel.StatusEnum? val = value as ViewModel.StatusEnum?;
			if (val == null || val == ViewModel.StatusEnum.EmptyProjectPath)
				return sb;
			switch (val)
			{
				case ViewModel.StatusEnum.InvalidDroidProject:
					sb = new SolidColorBrush(Colors.Red);
					break;
				case ViewModel.StatusEnum.ProcessSuccess:
				case ViewModel.StatusEnum.ValidDroidProject:
					sb = new SolidColorBrush(Colors.Green);
					break;
				case ViewModel.StatusEnum.Processing:
					sb = new SolidColorBrush(Colors.Blue);
					break;
				default:
					break;
			}
			return sb;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
