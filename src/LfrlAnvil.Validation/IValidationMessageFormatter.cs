using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

namespace LfrlAnvil.Validation;

public interface IValidationMessageFormatter<TResource>
{
    [Pure]
    string GetResourceTemplate(TResource resource, IFormatProvider? formatProvider);

    [Pure]
    ValidationMessageFormatterArgs GetArgs(IFormatProvider? formatProvider);

    [return: NotNullIfNotNull( "builder" )]
    StringBuilder? Format(StringBuilder? builder, Chain<ValidationMessage<TResource>> messages, IFormatProvider? formatProvider = null);
}
