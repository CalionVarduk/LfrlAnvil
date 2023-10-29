using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsElementCountInRangeValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    public IsElementCountInRangeValidator(int minCount, int maxCount, TResult failureResult)
    {
        Ensure.IsInRange( minCount, 0, maxCount );
        MinCount = minCount;
        MaxCount = maxCount;
        FailureResult = failureResult;
    }

    public int MinCount { get; }
    public int MaxCount { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        var count = obj.Count;
        return count >= MinCount && count <= MaxCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
