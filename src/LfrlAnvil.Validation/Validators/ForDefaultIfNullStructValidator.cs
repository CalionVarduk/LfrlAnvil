using System.Diagnostics.Contracts;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator which uses specified <see cref="DefaultValue"/> when validated object is null.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class ForDefaultIfNullStructValidator<T, TResult> : IValidator<T?, TResult>
    where T : struct
{
    /// <summary>
    /// Creates a new <see cref="ForDefaultIfNullStructValidator{T,TResult}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="defaultValue">Default value to use instead of a null object.</param>
    public ForDefaultIfNullStructValidator(IValidator<T, TResult> validator, T defaultValue)
    {
        Validator = validator;
        DefaultValue = defaultValue;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<T, TResult> Validator { get; }

    /// <summary>
    /// Default value to use instead of a null object.
    /// </summary>
    public T DefaultValue { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<TResult> Validate(T? obj)
    {
        return Validator.Validate( obj ?? DefaultValue );
    }
}
