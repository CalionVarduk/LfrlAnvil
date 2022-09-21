using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsNullValidator<T, TResult> : IValidator<T, TResult>
{
    public IsNullValidator(TResult failureResult)
    {
        FailureResult = failureResult;
    }

    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Generic<T>.IsNull( obj ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
