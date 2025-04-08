using FluentValidation;

namespace RateRelay.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithAppErrorCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, string errorCode)
    {
        return rule.WithErrorCode(errorCode);
    }
}