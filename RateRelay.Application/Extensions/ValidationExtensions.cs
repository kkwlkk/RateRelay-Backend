using FluentValidation;

namespace RateRelay.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithErrorCode2<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, string errorCode)
    {
        return rule.WithState(_ => errorCode);
    }
}