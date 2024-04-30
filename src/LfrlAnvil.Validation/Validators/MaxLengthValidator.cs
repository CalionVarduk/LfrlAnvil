using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects a maximum <see cref="String.Length"/>.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MaxLengthValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MaxLengthValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="maxLength">Expected maximum <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxLength"/> is less than <b>0</b>.</exception>
    public MaxLengthValidator(int maxLength, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( maxLength, 0 );
        MaxLength = maxLength;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected maximum <see cref="String.Length"/>.
    /// </summary>
    public int MaxLength { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length <= MaxLength ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
