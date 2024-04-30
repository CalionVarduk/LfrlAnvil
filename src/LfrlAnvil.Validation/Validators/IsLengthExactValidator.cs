using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects an exact <see cref="String.Length"/>.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsLengthExactValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsLengthExactValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="length">Expected exact <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length"/> is less than <b>0</b>.</exception>
    public IsLengthExactValidator(int length, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( length, 0 );
        Length = length;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected exact <see cref="String.Length"/>.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length == Length ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
