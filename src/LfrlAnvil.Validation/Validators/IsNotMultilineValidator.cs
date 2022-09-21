using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsNotMultilineValidator<TResult> : IValidator<string, TResult>
{
    public IsNotMultilineValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Contains( '\n' ) ? Chain.Create( FailureResult ) : Chain<TResult>.Empty;
    }
}
