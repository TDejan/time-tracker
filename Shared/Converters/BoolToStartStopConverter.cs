using System.Globalization;
using System.Windows.Data;

namespace TimeTracker.Shared.Converters
{
    public class BoolToStartStopConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "Stop" : "Start";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}