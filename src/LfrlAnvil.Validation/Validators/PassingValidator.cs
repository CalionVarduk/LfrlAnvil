using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class PassingValidator<T, TResult> : IValidator<T, TResult>
{
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return Chain<TResult>.Empty;
    }
}
