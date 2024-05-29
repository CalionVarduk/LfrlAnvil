using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Validation;

/// <inheritdoc cref="IValidationMessageFormatter{TResource}" />
public abstract class ValidationMessageFormatter<TResource> : IValidationMessageFormatter<TResource>
{
    /// <inheritdoc />
    [Pure]
    public abstract string GetResourceTemplate(TResource resource, IFormatProvider? formatProvider);

    /// <inheritdoc />
    [Pure]
    public abstract ValidationMessageFormatterArgs GetArgs(IFormatProvider? formatProvider);

    /// <inheritdoc />
    [return: NotNullIfNotNull( "builder" )]
    public StringBuilder? Format(
        StringBuilder? builder,
        Chain<ValidationMessage<TResource>> messages,
        IFormatProvider? formatProvider = null)
    {
        if ( messages.Count == 0 )
            return builder;

        builder ??= new StringBuilder();
        var args = GetArgs( formatProvider );

        if ( ! string.IsNullOrEmpty( args.PrefixAll ) )
            builder.AppendFormat( formatProvider, args.PrefixAll, messages.Count );

        var separator = args.Separator;

        var index = 1;
        foreach ( var message in messages )
        {
            var template = GetResourceTemplate( message.Resource, formatProvider );

            if ( args.IncludeIndex )
                AppendIndex( builder, index++, formatProvider );

            builder.Append( args.PrefixEach );
            builder.AppendFormat( formatProvider, template, message.Parameters ?? Array.Empty<object?>() );
            builder.Append( args.PostfixEach );
            builder.Append( separator );
        }

        builder.ShrinkBy( separator.Length ).Append( args.PostfixAll );
        return builder;
    }

    /// <summary>
    /// Appends a message index to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">String builder.</param>
    /// <param name="index">Message index to append.</param>
    /// <param name="formatProvider">Optional format provider.</param>
    protected virtual void AppendIndex(StringBuilder builder, int index, IFormatProvider? formatProvider)
    {
        builder.Append( index ).Append( '.' ).Append( ' ' );
    }
}
