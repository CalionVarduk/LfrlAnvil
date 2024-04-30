using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic collection of objects validator that expects a maximum number of elements.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MaxElementCountValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MaxElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="maxCount"/> is less than <b>0</b>.</exception>
    public MaxElementCountValidator(int maxCount, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( maxCount, 0 );
        MaxCount = maxCount;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected maximum number of elements.
    /// </summary>
    public int MaxCount { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count <= MaxCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
