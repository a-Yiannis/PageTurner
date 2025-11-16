using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;

namespace PageTurnerW.Converters;

public class TimeSpanEqualToVisibility:IValueConverter {
	public TimeSpan Target { get; set; }
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not TimeSpan timeSpan) throw new NotSupportedException("The target is not supported!");
		return (timeSpan - Target).TotalSeconds < 0.1 ? Visibility.Collapsed : Visibility.Visible;
	}
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException("One-way conversion only");
}
