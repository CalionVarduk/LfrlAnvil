using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that expects objects to be less than a specific value.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsLessThanValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsLessThanValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="determinant">Value to compare with.</param>
    /// <param name="comparer">Value comparer.</param>
    /// <param name="failureResult">Failure result.</param>
    public IsLessThanValidator(T determinant, IComparer<T> comparer, TResult failureResult)
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
        return Comparer.Compare( obj, Determinant ) < 0 ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
