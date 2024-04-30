using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects a single-line string.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsNotMultilineValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsNotMultilineValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="failureResult">Failure result.</param>
    public IsNotMultilineValidator(TResult failureResult)
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
        return obj.Contains( '\n' ) ? Chain.Create( FailureResult ) : Chain<TResult>.Empty;
    }
}
