using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects a minimum <see cref="String.Length"/>.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MinLengthValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MinLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Expected minimum <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minLength"/> is less than <b>0</b>.</exception>
    public MinLengthValidator(int minLength, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( minLength, 0 );
        MinLength = minLength;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected minimum <see cref="String.Length"/>.
    /// </summary>
    public int MinLength { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length >= MinLength ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
