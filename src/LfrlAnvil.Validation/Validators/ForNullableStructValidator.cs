using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class ForNullableStructValidator<T, TResult> : IValidator<T?, TResult>
    where T : struct
{
    public ForNullableStructValidator(IValidator<T, TResult> validator)
    {
        Validator = validator;
    }

    public IValidator<T, TResult> Validator { get; }

    [Pure]
    public Chain<TResult> Validate(T? obj)
    {
        return obj is null ? Chain<TResult>.Empty : Validator.Validate( obj.Value );
    }
}
