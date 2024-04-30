using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects <see cref="String.Length"/> between a given range.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsLengthInRangeValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsLengthInRangeValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="minLength">Minimum expected <see cref="String.Length"/>.</param>
    /// <param name="maxLength">Maximum expected <see cref="String.Length"/>.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minLength"/> is not in [<b>0</b>, <paramref name="maxLength"/>] range.
    /// </exception>
    public IsLengthInRangeValidator(int minLength, int maxLength, TResult failureResult)
    {
        Ensure.IsInRange( minLength, 0, maxLength );
        MinLength = minLength;
        MaxLength = maxLength;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Minimum expected <see cref="String.Length"/>.
    /// </summary>
    public int MinLength { get; }

    /// <summary>
    /// Maximum expected <see cref="String.Length"/>.
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
        return obj.Length >= MinLength && obj.Length <= MaxLength ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
