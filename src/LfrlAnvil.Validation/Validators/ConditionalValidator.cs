using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class ConditionalValidator<T, TResult> : IValidator<T, TResult>
{
    public ConditionalValidator(Func<T, bool> condition, IValidator<T, TResult> ifTrue, IValidator<T, TResult> ifFalse)
    {
        Condition = condition;
        IfTrue = ifTrue;
        IfFalse = ifFalse;
    }

    public Func<T, bool> Condition { get; }
    public IValidator<T, TResult> IfTrue { get; }
    public IValidator<T, TResult> IfFalse { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var validator = Condition( obj ) ? IfTrue : IfFalse;
        return validator.Validate( obj );
    }
}
