using System.ComponentModel.DataAnnotations;

namespace RateRelay.Infrastructure.Attributes;

public class TimeSpanRangeAttribute(string minimum, string maximum) : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(minimum) || string.IsNullOrEmpty(maximum))
        {
            return new ValidationResult("Minimum and maximum values must be provided.");
        }

        var minSpan = TimeSpan.Parse(minimum);
        var maxSpan = TimeSpan.Parse(maximum);

        if (value == null) return ValidationResult.Success;

        if (value is not TimeSpan timeSpan)
        {
            return new ValidationResult($"The {validationContext.DisplayName} field must be a TimeSpan.");
        }

        if (timeSpan < minSpan || timeSpan > maxSpan)
        {
            return new ValidationResult(
                $"The {validationContext.DisplayName} field must be between {minSpan} and {maxSpan}.");
        }

        return ValidationResult.Success;
    }
}