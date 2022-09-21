using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class SwitchValidator<T, TResult, TSwitchValue> : IValidator<T, TResult>
    where TSwitchValue : notnull
{
    public SwitchValidator(
        Func<T, TSwitchValue> switchValueSelector,
        IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> validators,
        IValidator<T, TResult> defaultValidator)
    {
        SwitchValueSelector = switchValueSelector;
        Validators = validators;
        DefaultValidator = defaultValidator;
    }

    public Func<T, TSwitchValue> SwitchValueSelector { get; }
    public IReadOnlyDictionary<TSwitchValue, IValidator<T, TResult>> Validators { get; }
    public IValidator<T, TResult> DefaultValidator { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var switchValue = SwitchValueSelector( obj );
        if ( ! Validators.TryGetValue( switchValue, out var validator ) )
            validator = DefaultValidator;

        return validator.Validate( obj );
    }
}
