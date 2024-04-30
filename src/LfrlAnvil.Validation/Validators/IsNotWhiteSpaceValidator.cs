using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects a string that does not contain white-space characters only.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsNotWhiteSpaceValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsNotWhiteSpaceValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    public IsNotWhiteSpaceValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return ! string.IsNullOrWhiteSpace( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
