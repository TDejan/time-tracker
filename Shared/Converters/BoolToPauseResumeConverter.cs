using System.Globalization;
using System.Windows.Data;

namespace TimeTracker.Shared.Converters
{
	public class BoolToPauseResumeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (bool)value ? "Resume" : "Pause";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
			throw new NotImplementedException();
	}
}
