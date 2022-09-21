using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsNotNullValidator<T, TResult> : IValidator<T, TResult>
{
    public IsNotNullValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Generic<T>.IsNotNull( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
