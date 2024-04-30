using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic collection of objects validator that expects a minimum number of elements.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class MinElementCountValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    /// <summary>
    /// Creates a new <see cref="MinElementCountValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="minCount">Expected minimum number of elements.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="minCount"/> is less than <b>0</b>.</exception>
    public MinElementCountValidator(int minCount, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( minCount, 0 );
        MinCount = minCount;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Expected minimum number of elements.
    /// </summary>
    public int MinCount { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count >= MinCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
