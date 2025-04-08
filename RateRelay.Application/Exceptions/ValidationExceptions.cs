using FluentValidation.Results;
using RateRelay.Application.Extensions;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Exceptions;

public class ValidationException : Exception
{
    public List<ValidationError> ValidationErrors { get; }

    public ValidationException(ValidationResult validationResult)
        : base("Validation failed")
    {
        ValidationErrors = validationResult.ToValidationErrors();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("Validation failed")
    {
        ValidationErrors = failures.Select(failure => new ValidationError
        {
            Property = failure.PropertyName,
            Message = failure.ErrorMessage,
            Code = failure.ErrorCode,
            AttemptedValue = failure.AttemptedValue
        }).ToList();
    }
}