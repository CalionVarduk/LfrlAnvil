using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class AndCompositeValidator<T, TResult> : IValidator<T, TResult>
{
    public AndCompositeValidator(IReadOnlyList<IValidator<T, TResult>> validators)
    {
        Validators = validators;
    }

    public IReadOnlyList<IValidator<T, TResult>> Validators { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        var result = Chain<TResult>.Empty;

        var count = Validators.Count;
        for ( var i = 0; i < count; ++i )
        {
            var validator = Validators[i];
            result = result.Extend( validator.Validate( obj ).ToExtendable() );
        }

        return result;
    }
}
