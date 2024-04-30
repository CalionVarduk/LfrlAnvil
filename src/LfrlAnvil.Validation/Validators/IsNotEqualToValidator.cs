using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that expects objects to not be equal to a specific value.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsNotEqualToValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsNotEqualToValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value equality comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    public IsNotEqualToValidator(T determinant, IEqualityComparer<T> comparer, TResult failureResult)
    {
        Determinant = determinant;
        Comparer = comparer;
        FailureResult = failureResult;
    }

    /// <summary>
    /// Value to compare with.
    /// </summary>
    public T Determinant { get; }

    /// <summary>
    /// Value equality comparer.
    /// </summary>
    public IEqualityComparer<T> Comparer { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return ! Comparer.Equals( obj, Determinant ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
