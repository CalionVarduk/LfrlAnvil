using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class MinElementCountValidator<T, TResult> : IValidator<IReadOnlyCollection<T>, TResult>
{
    public MinElementCountValidator(int minCount, TResult failureResult)
    {
        Ensure.IsGreaterThanOrEqualTo( minCount, 0 );
        MinCount = minCount;
        FailureResult = failureResult;
    }

    public int MinCount { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(IReadOnlyCollection<T> obj)
    {
        return obj.Count >= MinCount ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
