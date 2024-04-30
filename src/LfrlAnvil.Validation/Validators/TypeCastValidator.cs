using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator that casts validated objects to <typeparamref name="TDestination"/> type
/// and performs conditional validation based on that type cast.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TDestination">Object type required for <see cref="IfIsOfType"/> validator invocation.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class TypeCastValidator<T, TDestination, TResult> : IValidator<T, TResult>
{
    /// <summary>
    /// Creates a new <see cref="TypeCastValidator{T,TDestination,TResult}"/> instance.
    /// </summary>
    /// <param name="ifIsOfType">Underlying validator invoked when validated object is of <typeparamref name="TDestination"/> type.</param>
    /// <param name="ifIsNotOfType">
    /// Underlying validator invoked when validated object is not of <typeparamref name="TDestination"/> type.
    /// </param>
    public TypeCastValidator(IValidator<TDestination, TResult> ifIsOfType, IValidator<T, TResult> ifIsNotOfType)
    {
        IfIsOfType = ifIsOfType;
        IfIsNotOfType = ifIsNotOfType;
    }

    /// <summary>
    /// Underlying validator invoked when validated object is of <typeparamref name="TDestination"/> type.
    /// </summary>
    public IValidator<TDestination, TResult> IfIsOfType { get; }

    /// <summary>
    /// Underlying validator invoked when validated object is not of <typeparamref name="TDestination"/> type.
    /// </summary>
    public IValidator<T, TResult> IfIsNotOfType { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T obj)
    {
        return obj is TDestination dest ? IfIsOfType.Validate( dest ) : IfIsNotOfType.Validate( obj );
    }
}
