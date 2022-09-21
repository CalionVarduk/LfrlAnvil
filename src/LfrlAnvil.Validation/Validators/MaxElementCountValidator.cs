using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class MaxElementCountValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    public MaxElementCountValidator(int maxCount, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( maxCount, 0, nameof( maxCount ) );
        MaxCount = maxCount;
        FailureResult = failureResult;
    }

    public int MaxCount { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count <= MaxCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
