using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class FailingValidator<T, TResult> : IValidator<T, TResult>
{
    public FailingValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Chain.Create( FailureResult );
    }
}
