﻿using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class ForDefaultIfNullRefValidator<T, TResult> : IValidator<T?, TResult>
    where T : class
{
    public ForDefaultIfNullRefValidator(IValidator<T, TResult> validator, T defaultValue)
    {
        Validator = validator;
        DefaultValue = defaultValue;
    }

    public IValidator<T, TResult> Validator { get; }
    public T DefaultValue { get; }

    [Pure]
    public Chain<TResult> Validate(T? obj)
    {
        return Validator.Validate( obj ?? DefaultValue );
    }
}