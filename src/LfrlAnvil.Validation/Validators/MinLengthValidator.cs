using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class MinLengthValidator<TResult> : IValidator<string, TResult>
{
    public MinLengthValidator(int minLength, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( minLength, 0 );
        MinLength = minLength;
        FailureResult = failureResult;
    }

    public int MinLength { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length >= MinLength ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
