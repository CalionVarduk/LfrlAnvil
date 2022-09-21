using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Validation.Extensions;

namespace LfrlAnvil.Validation.Validators;

public sealed class FormattedValidator<T, TResource> : IValidator<T, FormattedValidatorResult<TResource>>
{
    public FormattedValidator(
        IValidator<T, ValidationMessage<TResource>> validator,
        IValidationMessageFormatter<TResource> formatter,
        Func<IFormatProvider?>? formatProvider = null)
    {
        Validator = validator;
        Formatter = formatter;
        FormatProvider = formatProvider;
    }

    public IValidator<T, ValidationMessage<TResource>> Validator { get; }
    public IValidationMessageFormatter<TResource> Formatter { get; }
    public Func<IFormatProvider?>? FormatProvider { get; }

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
