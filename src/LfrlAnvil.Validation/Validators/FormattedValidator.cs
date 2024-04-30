using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a generic object validator with formatted messages.
/// </summary>
/// <typeparam name="T">Object type.</typeparam>
/// <typeparam name="TResource">Message's resource type.</typeparam>
public sealed class FormattedValidator<T, TResource> : IValidator<T, FormattedValidatorResult<TResource>>
{
    /// <summary>
    /// Creates a new <see cref="FormattedValidator{T,TResource}"/> instance.
    /// </summary>
    /// <param name="validator">Underlying validator.</param>
    /// <param name="formatter">Underlying validation message formatter.</param>
    /// <param name="formatProvider">Optional format provider factory.</param>
    public FormattedValidator(
        IValidator<T, ValidationMessage<TResource>> validator,
        IValidationMessageFormatter<TResource> formatter,
        Func<IFormatProvider?>? formatProvider = null)
    {
        Validator = validator;
        Formatter = formatter;
        FormatProvider = formatProvider;
    }

    /// <summary>
    /// Underlying validator.
    /// </summary>
    public IValidator<T, ValidationMessage<TResource>> Validator { get; }

    /// <summary>
    /// Underlying validation message formatter.
    /// </summary>
    public IValidationMessageFormatter<TResource> Formatter { get; }

    /// <summary>
    /// Optional format provider factory.
    /// </summary>
    public Func<IFormatProvider?>? FormatProvider { get; }

    /// <inheritdoc />
    [Pure]
    public Chain<FormattedValidatorResult<TResource>> Validate(T obj)
    {
        var messages = Validator.Validate( obj );
        var formatProvider = FormatProvider?.Invoke();
        var builder = Formatter.Format( messages, formatProvider );

        return builder is null
            ? Chain<FormattedValidatorResult<TResource>>.Empty
            : Chain.Create( new FormattedValidatorResult<TResource>( messages, builder.ToString() ) );
    }
}
