using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsInExclusiveRangeValidator<T, TResult> : IValidator<T, TResult>
{
    public IsInExclusiveRangeValidator(T min, T max, IComparer<T> comparer, TResult failureResult)
    {
        Ensure.IsLessThanOrEqualTo( min, max, comparer );
        Min = min;
        Max = max;
        Comparer = comparer;
        FailureResult = failureResult;
    }

    public T Min { get; }
    public T Max { get; }
    public IComparer<T> Comparer { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Comparer.Compare( obj, Min ) > 0 && Comparer.Compare( obj, Max ) < 0
            ? Chain<TResult>.Empty
            : Chain.Create( FailureResult );
    }
}
