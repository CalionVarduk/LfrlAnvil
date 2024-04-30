using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic collection of objects validator that expects an exact number of elements.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsElementCountExactValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsElementCountExactValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="count">Expected exact number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    public IsElementCountExactValidator(int count, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0 );
        Count = count;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected exact number of elements.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count == Count ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
