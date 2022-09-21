using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsLessThanOrEqualToValidator<T, TResult> : IValidator<T, TResult>
{
    public IsLessThanOrEqualToValidator(T determinant, IComparer<T> comparer, TResult failureResult)
    {
        Determinant = determinant;
        Comparer = comparer;
        FailureResult = failureResult;
    }

    public T Determinant { get; }
    public IComparer<T> Comparer { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Comparer.Compare( obj, Determinant ) <= 0 ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
