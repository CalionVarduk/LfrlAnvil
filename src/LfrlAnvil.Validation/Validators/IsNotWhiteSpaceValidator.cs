using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsNotWhiteSpaceValidator<TResult> : IValidator<string, TResult>
{
    public IsNotWhiteSpaceValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return ! string.IsNullOrWhiteSpace( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
