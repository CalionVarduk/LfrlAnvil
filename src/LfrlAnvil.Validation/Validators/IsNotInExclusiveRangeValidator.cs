using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that expects objects to not be in a specified exclusive range.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsNotInExclusiveRangeValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsNotInExclusiveRangeValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="min">Minimum exclusive value to compare with.</param>
    /// <param name="max">Maximum exclusive value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
    public IsNotInExclusiveRangeValidator(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        Ensure.IsLessThanOrEqualTo( min, max, comparer );
        Min = min;
        Max = max;
        Comparer = comparer;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Minimum exclusive value to compare with.
    /// </summary>
    public T Min { get; }

    /// <summary>
    /// Maximum exclusive value to compare with.
    /// </summary>
    public T Max { get; }

    /// <summary>
    /// Value comparer.
    /// </summary>
    public IComparer<T> Comparer { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Comparer.Compare( obj, Min ) <= 0 || Comparer.Compare( obj, Max ) >= 0
            ? Chain<TResult>.Empty
            : Chain.Create( FailureResult );
    }
}
