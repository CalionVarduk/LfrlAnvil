using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

public readonly struct FormattedValidatorResult<TResource>
{
    private readonly string? _result;

    public FormattedValidatorResult(Chain<ValidationMessage<TResource>> messages, string result)
    {
        Messages = messages;
        _result = result;
    }

    public Chain<ValidationMessage<TResource>> Messages { get; }
    public string Result => _result ?? string.Empty;

    [Pure]
    public override string ToString()
    {
        var messagesText = string.Join( Environment.NewLine, Messages.Select( (m, i) => $"{i + 1}. '{m}'" ) );
        return $"{nameof( Result )}: '{Result}', {nameof( Messages )}:{Environment.NewLine}{messagesText}";
    }
}
