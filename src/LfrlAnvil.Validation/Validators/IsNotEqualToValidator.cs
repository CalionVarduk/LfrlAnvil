using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class IsNotEqualToValidator<T, TResult> : IValidator<T, TResult>
{
    public IsNotEqualToValidator(T determinant, IEqualityComparer<T> comparer, TResult failureResult)
    {
        Determinant = determinant;
        Comparer = comparer;
        FailureResult = failureResult;
    }

    public T Determinant { get; }
    public IEqualityComparer<T> Comparer { get; }
    public TResult FailureResult { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return ! Comparer.Equals( obj, Determinant ) ? Chain<TResult>.Empty : Chain.Create( FailureResult );
    }
}
