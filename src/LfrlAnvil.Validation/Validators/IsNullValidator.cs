using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that expects objects to be null.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsNullValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsNullValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    public IsNullValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Generic<T>.IsNull( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
