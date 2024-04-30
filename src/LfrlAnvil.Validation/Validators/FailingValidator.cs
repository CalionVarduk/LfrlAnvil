using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that always fails.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class FailingValidator<T, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="FailingValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    public FailingValidator(TResult failureResult)
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
        return Chain.Create( FailureResult );
    }
}
