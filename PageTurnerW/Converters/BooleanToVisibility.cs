using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PageTurnerW.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibility : IValueConverter {
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is bool boolValue && boolValue ? Visibility.Visible : Visibility.Collapsed;

	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException("One-way conversion only");
}
