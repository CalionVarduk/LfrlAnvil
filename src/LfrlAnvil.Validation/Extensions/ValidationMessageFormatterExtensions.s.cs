using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;

namespace LfrlAnvil.Validation.Extensions;

public static class ValidationMessageFormatterExtensions
{
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
