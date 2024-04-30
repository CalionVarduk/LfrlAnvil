using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a collection of generic object validators that returns an empty result
/// if any of the underlying validators returns an empty result.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class OrCompositeValidator<T, TResult> : IValidator<T, OrValidatorResult<TResult>>
{
    /// <summary>
    /// Creates a new <see cref="OrCompositeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validators">Underlying validators.</param>
    public OrCompositeValidator(IReadOnlyList<IValidator<T, TResult>> validators)
    {
        Validators = validators;
    }

    /// <summary>
    /// Underlying validators.
    /// </summary>
    public IReadOnlyList<IValidator<T, TResult>> Validators { get; }

    /// <inheritdoc />
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
