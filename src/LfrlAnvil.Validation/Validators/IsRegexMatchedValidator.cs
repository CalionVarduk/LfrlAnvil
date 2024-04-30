using System;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a <see cref="String"/> validator that expects a <see cref="Regex"/> to be matched.
/// </summary>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class IsRegexMatchedValidator<TResult> : IValidator<string, TResult>
{
    /// <summary>
    /// Creates a new <see cref="IsRegexMatchedValidator{TResult}"/> instance.
    /// </summary>
    /// <param name="regex">Regex to match.</param>
    /// <param name="failureResult">Failure result.</param>
    public IsRegexMatchedValidator(Regex regex, TResult failureResult)
    {
        Regex = regex;
        FailureResult = failureResult;
    }

    /// <summary>
    /// <see cref="Regex"/> to match.
    /// </summary>
    public Regex Regex { get; }

    /// <summary>
    /// Failure result.
    /// </summary>
    public TResult FailureResult { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return Regex.IsMatch( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
