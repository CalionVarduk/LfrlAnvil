using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class OrCompositeValidator<T, TResult> : IValidator<T, OrValidatorResult<TResult>>
{
    public OrCompositeValidator(IReadOnlyList<IValidator<T, TResult>> validators)
    {
        Validators = validators;
    }

    public IReadOnlyList<IValidator<T, TResult>> Validators { get; }

    [Pure]
    public Chain<OrValidatorResult<TResult>> Validate(T obj)
    {
        var result = Chain<TResult>.Empty;

        var count = Validators.Count;
        for ( var i = 0; i < count; ++i )
        {
            var validator = Validators[i];
            var next = validator.Validate( obj );
            if ( next.Count == 0 )
                return Chain<OrValidatorResult<TResult>>.Empty;

            result = result.Extend( next.ToExtendable() );
        }

        return Chain.Create( new OrValidatorResult<TResult>( result ) );
    }
}
