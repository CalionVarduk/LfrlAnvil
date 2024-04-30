using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a collection of generic object validators where each validator is invoked and their results are concatenated together.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class AndCompositeValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="AndCompositeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validators">Underlying validators.</param>
    public AndCompositeValidator(IReadOnlyList<IValidator<T, TResult>> validators)
    {
        Validators = validators;
    }

    /// <summary>
    /// Underlying validators.
    /// </summary>
    public IReadOnlyList<IValidator<T, TResult>> Validators { get; }

    /// <inheritdoc />
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
