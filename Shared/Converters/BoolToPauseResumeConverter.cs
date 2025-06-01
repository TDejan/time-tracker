using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TimeTracker.Shared.Converters
{
	public class BoolToPauseResumeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			bool isPaused = (bool)value;

			var uri = isPaused
				? new Uri("pack://application:,,,/assets/icons/resume.ico")
				: new Uri("pack://application:,,,/assets/icons/pause.ico");

			return new BitmapImage(uri);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
