using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsElementCountExactValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    public IsElementCountExactValidator(int count, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( count, 0, nameof( count ) );
        Count = count;
        FailureResult = failureResult;
    }

    public int Count { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count == Count ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
