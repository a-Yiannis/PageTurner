using System.Globalization;
using System.Numerics;
using System.Windows;
using System.Windows.Data;

namespace PageTurnerW.Converters;

/// <summary>
/// Converts numeric values and strings to Visibility based on whether they represent zero or empty values.
/// Optimized for common UI binding scenarios with fast-path handling for typical zero representations.
/// </summary>
public class NumToVisibilityConverter : IValueConverter {
	/// <summary>Whitespace and zero characters and zero used for trimming</summary>
	const string WSandZero = "0 \t\r\n";

	/// <summary>Pre-computed set of whitespace characters and zero for single-character checks</summary>
	static readonly HashSet<char> whiteSpacesAndZeroSet = [ ..WSandZero ];

	/// <summary>
	/// Determines if a string represents a zero value that should be collapsed.
	/// Handles common patterns like "0", "0.0", ".0", "0.", and whitespace-only strings.
	/// </summary>
	/// <param name="str">The string to evaluate (cannot be null)</param>
	/// <param name="c">Culture info for decimal separator, uses invariant culture if null</param>
	/// <returns>True if the string represents zero or empty value and should be collapsed</returns>
	static bool ShouldBeCollapsed(string str, CultureInfo? c) {
		if (str.Length == 0) return true;
		if (str.Length == 1) return str[0] == '0' || whiteSpacesAndZeroSet.Contains(str[0]);

		char sep = c?.NumberFormat.NumberDecimalSeparator[0] ?? '.';
		if (str.Length == 3 && str[0] == '0' && str[1] == sep && str[2] == '0')
			return true;

		var s = str.AsSpan().Trim(WSandZero);
		return s.Length switch {
			0 => true, // String contained only zeros and whitespace
			1 => s[0] == sep, // Only decimal separator remains (e.g., "0.0" trimmed to ".")
			_ => false
		};
	}

	/// <summary>
	/// Converts various value types to Visibility.Collapsed if they represent zero/empty values.
	/// Supported types: double, int, string, TimeSpan, and null.
	/// </summary>
	/// <param name="value">The value to convert</param>
	/// <param name="targetType">The target type (should be Visibility)</param>
	/// <param name="parameter">Optional parameter (unused)</param>
	/// <param name="culture">Culture info for string parsing</param>
	/// <returns>Visibility.Collapsed for zero/empty values, Visibility.Visible otherwise</returns>
	public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture) => value switch {
		double d => d == 0,
		int i => i == 0,
		null => true,
		string s => ShouldBeCollapsed(s, culture),
		TimeSpan ts => ts.Ticks == 0,
		IConvertible convertible => convertible.ToDouble(culture ?? CultureInfo.CurrentUICulture) == 0,
		_ => false
	} ? Visibility.Collapsed : Visibility.Visible;

	/// <summary>
	/// Not implemented - this is a one-way converter.
	/// </summary>
	public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotSupportedException("One-way conversion only");
}
