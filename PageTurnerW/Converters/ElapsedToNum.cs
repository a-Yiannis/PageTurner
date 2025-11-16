using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace PageTurnerW.Converters;

public class ElapsedToNum:IValueConverter {
	static readonly TimeSpan HoursThreshold = TimeSpan.FromHours(1);
	static readonly TimeSpan MinutesThreshold = TimeSpan.FromMinutes(1);
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
		if (value is not TimeSpan ts) throw new InvalidDataException($"Expected {nameof(TimeSpan)} got {value?.GetType()}.");
		if (ts > HoursThreshold) return ts.TotalHours.ToString("N2") + " hours";
		if (ts > MinutesThreshold) return ts.TotalMinutes.ToString("N1") + " mins";
		return ts.TotalSeconds.ToString("N0")+"s";
	}
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException("One-way conversion only");
}
