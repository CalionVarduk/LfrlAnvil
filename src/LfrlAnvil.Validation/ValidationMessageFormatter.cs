using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;

namespace LfrlAnvil.Validation;

public abstract class ValidationMessageFormatter<TResource> : IValidationMessageFormatter<TResource>
{
    [Pure]
    public abstract string GetResourceTemplate(TResource resource, IFormatProvider? formatProvider);

    [Pure]
    public abstract ValidationMessageFormatterArgs GetArgs(IFormatProvider? formatProvider);

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
            builder.AppendFormat( formatProvider, template, message.Parameters );
            builder.Append( args.PostfixEach );
            builder.Append( separator );
        }

        builder.Length -= separator.Length;
        builder.Append( args.PostfixAll );

        return builder;
    }

    protected virtual void AppendIndex(StringBuilder builder, int index, IFormatProvider? formatProvider)
    {
        builder.Append( index ).Append( '.' ).Append( ' ' );
    }
}
