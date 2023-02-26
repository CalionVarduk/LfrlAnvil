using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public static class ParsedExpressionDelegateExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionBufferedDelegate<TArg, TResult> ToBuffered<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source)
    {
        return new ParsedExpressionBufferedDelegate<TArg, TResult>( source );
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        var argumentCount = source.Arguments.Count;
        var buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
        source.MapArguments( buffer, arguments );
        return buffer;
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        params KeyValuePair<string, TArg?>[] arguments)
    {
        return source.MapArguments( arguments.AsEnumerable() );
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        IEnumerable<KeyValuePair<StringSlice, TArg?>> arguments)
    {
        var argumentCount = source.Arguments.Count;
        var buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
        source.MapArguments( buffer, arguments );
        return buffer;
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        params KeyValuePair<StringSlice, TArg?>[] arguments)
    {
        return source.MapArguments( arguments.AsEnumerable() );
    }

    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        MapArguments( source, buffer, arguments.Select( static kv => KeyValuePair.Create( kv.Key.AsSlice(), kv.Value ) ) );
    }

    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        params KeyValuePair<string, TArg?>[] arguments)
    {
        source.MapArguments( buffer, arguments.AsEnumerable() );
    }

    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<StringSlice, TArg?>> arguments)
    {
        var argumentCount = source.Arguments.Count;
        if ( argumentCount == 0 )
            return;

        if ( buffer.Length < argumentCount )
            throw new ParsedExpressionArgumentBufferTooSmallException( buffer.Length, argumentCount, nameof( buffer ) );

        var invalidArgumentNames = Chain<StringSlice>.Empty;

        foreach ( var (name, value) in arguments )
        {
            var index = source.Arguments.GetIndex( name );
            if ( index < 0 )
            {
                invalidArgumentNames = invalidArgumentNames.Extend( name );
                continue;
            }

            buffer[index] = value;
        }

        if ( invalidArgumentNames.Count > 0 )
            throw new InvalidParsedExpressionArgumentsException( invalidArgumentNames, nameof( arguments ) );
    }

    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        params KeyValuePair<StringSlice, TArg?>[] arguments)
    {
        source.MapArguments( buffer, arguments.AsEnumerable() );
    }
}
