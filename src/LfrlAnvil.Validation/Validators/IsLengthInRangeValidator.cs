using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsLengthInRangeValidator<TResult> : IValidator<string, TResult>
{
    public IsLengthInRangeValidator(int minLength, int maxLength, TResult failureResult)
    {
        Ensure.IsInRange( minLength, 0, maxLength );
        MinLength = minLength;
        MaxLength = maxLength;
        FailureResult = failureResult;
    }

    public int MinLength { get; }
    public int MaxLength { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length >= MinLength && obj.Length <= MaxLength ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
