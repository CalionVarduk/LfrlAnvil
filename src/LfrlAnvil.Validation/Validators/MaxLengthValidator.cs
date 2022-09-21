using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class MaxLengthValidator<TResult> : IValidator<string, TResult>
{
    public MaxLengthValidator(int maxLength, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( maxLength, 0, nameof( maxLength ) );
        MaxLength = maxLength;
        FailureResult = failureResult;
    }

    public int MaxLength { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length <= MaxLength ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
