using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsRegexMatchedValidator<TResult> : IValidator<string, TResult>
{
    public IsRegexMatchedValidator(Regex regex, TResult failureResult)
    {
        Regex = regex;
        FailureResult = failureResult;
    }

    public Regex Regex { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return Regex.IsMatch( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
