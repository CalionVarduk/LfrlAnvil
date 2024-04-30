using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a result of <see cref="OrCompositeValidator{T,TResult}"/> validator.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public readonly struct OrValidatorResult<TResult>
{
    /// <summary>
    /// Creates a new <see cref="OrValidatorResult{TResult}"/> instance.
    /// </summary>
    /// <param name="result">Underlying result.</param>
    public OrValidatorResult(Chain<TResult> result)
    {
        Result = result;
    }

    /// <summary>
    /// Underlying result.
    /// </summary>
    public Chain<TResult> Result { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="OrValidatorResult{TResult}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return string.Join( $" or{Environment.NewLine}", Result.Select( static r => $"'{r}'" ) );
    }
}
