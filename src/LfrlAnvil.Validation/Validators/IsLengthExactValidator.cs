using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsLengthExactValidator<TResult> : IValidator<string, TResult>
{
    public IsLengthExactValidator(int length, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( length, 0, nameof( length ) );
        Length = length;
        FailureResult = failureResult;
    }

    public int Length { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(string obj)
    {
        return obj.Length == Length ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
