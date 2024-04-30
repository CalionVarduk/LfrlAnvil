using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Validation.Extensions;

/// <summary>
/// Contains <see cref="IValidationMessageFormatter{TResource}"/> extension methods.
/// </summary>
public static class ValidationMessageFormatterExtensions
{
    /// <summary>
    /// Formats the provided sequence of <paramref name="messages"/>.
    /// </summary>
    /// <param name="formatter">Source validation message formatter.</param>
    /// <param name="messages">Sequence of messages to format.</param>
    /// <param name="formatProvider">Optional format provider.</param>
    /// <returns>New <see cref="StringBuilder"/> instance or null when <paramref name="messages"/> are empty.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StringBuilder? Format<TResource>(
        this IValidationMessageFormatter<TResource> formatter,
        Chain<ValidationMessage<TResource>> messages,
        IFormatProvider? formatProvider = null)
    {
        return formatter.Format( builder: null, messages, formatProvider );
    }
}
