using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

public sealed class TypeCastValidator<T, TDestination, TResult> : IValidator<T, TResult>
{
    public TypeCastValidator(IValidator<TDestination, TResult> ifIsOfType, IValidator<T, TResult> ifIsNotOfType)
    {
        IfIsOfType = ifIsOfType;
        IfIsNotOfType = ifIsNotOfType;
    }

    public IValidator<TDestination, TResult> IfIsOfType { get; }
    public IValidator<T, TResult> IfIsNotOfType { get; }

    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return obj is TDestination dest ? IfIsOfType.Validate( dest ) : IfIsNotOfType.Validate( obj );
    }
}
