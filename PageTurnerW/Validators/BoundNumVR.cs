using System.Globalization;
using System.Windows.Controls;

namespace PageTurnerW.Validators;

public class BoundNumVR : ValidationRule
{
    public double Min { get; set; }
    public double Max { get; set; }

    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is not string strValue)
        {
            return new ValidationResult(false, "Value must be a string.");
        }

        if (!double.TryParse(strValue, NumberStyles.Any, cultureInfo, out double doubleValue))
        {
            return new ValidationResult(false, "Must be a valid number.");
        }

        if (doubleValue < Min || doubleValue > Max)
        {
            return new ValidationResult(false, $"Must be between {Min} and {Max}.");
        }

        return ValidationResult.ValidResult;
    }
}