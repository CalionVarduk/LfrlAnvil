// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
