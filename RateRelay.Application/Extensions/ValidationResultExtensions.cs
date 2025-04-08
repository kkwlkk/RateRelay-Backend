using FluentValidation.Results;
using RateRelay.Domain.Common;

namespace RateRelay.Application.Extensions;

public static class ValidationResultExtensions
{
    public static List<ValidationError> ToValidationErrors(this ValidationResult validationResult)
    {
        return validationResult.Errors.Select(error => new ValidationError
        {
            Property = error.PropertyName,
            Message = error.ErrorMessage,
            Code = error.ErrorCode,
            AttemptedValue = error.AttemptedValue
        }).ToList();
    }
}