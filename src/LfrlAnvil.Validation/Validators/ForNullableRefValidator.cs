using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic nullable object validator where null objects are considered valid.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class ForNullableRefValidator<T, TResult> : IValidator<T?, TResult>
    where T : class
{
    /// <summary>
    /// Creates a new <see cref="ForNullableRefValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    public ForNullableRefValidator(IValidator<T, TResult> validator)
    {
        Validator = validator;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<T, TResult> Validator { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T? obj)
    {
        return obj is null ? Chain<TResult>.Empty : Validator.Validate( obj );
    }
}
