using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Validation.Validators;

/// <summary>
/// Represents a formatted validation result created from a sequence of generic <see cref="ValidationMessage{TResource}"/> instances.
/// </summary>
/// <typeparam name="TResource">Resource type.</typeparam>
public readonly struct FormattedValidatorResult<TResource>
{
    private readonly string? _result;

    /// <summary>
    /// Creates a new <see cref="FormattedValidatorResult{TResource}"/> instance.
    /// </summary>
    /// <param name="messages">Validation result.</param>
    /// <param name="result">Formatted message.</param>
    public FormattedValidatorResult(Chain<ValidationMessage<TResource>> messages, string result)
    {
        Messages = messages;
        _result = result;
    }

    /// <summary>
    /// Validation result.
    /// </summary>
    public Chain<ValidationMessage<TResource>> Messages { get; }

    /// <summary>
    /// Formatted message.
    /// </summary>
    public string Result => _result ?? string.Empty;

    /// <summary>
    /// Returns a string representation of this <see cref="FormattedValidatorResult{TResource}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var messagesText = string.Join( Environment.NewLine, Messages.Select( static (m, i) => $"{i + 1}. '{m}'" ) );
        return $"{nameof( Result )}: '{Result}', {nameof( Messages )}:{Environment.NewLine}{messagesText}";
    }
}
