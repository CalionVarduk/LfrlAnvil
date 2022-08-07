using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Extensions;

public static class MathExpressionDelegateExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MathExpressionBufferedDelegate<TArg, TResult> ToBuffered<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source)
    {
        return new MathExpressionBufferedDelegate<TArg, TResult>( source );
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        var argumentCount = source.GetArgumentCount();
        var buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
        source.MapArguments( buffer, arguments );
        return buffer;
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        params KeyValuePair<string, TArg?>[] arguments)
    {
        return source.MapArguments( arguments.AsEnumerable() );
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TArg?>> arguments)
    {
        var argumentCount = source.GetArgumentCount();
        var buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
        source.MapArguments( buffer, arguments );
        return buffer;
    }

    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        params KeyValuePair<ReadOnlyMemory<char>, TArg?>[] arguments)
    {
        return source.MapArguments( arguments.AsEnumerable() );
    }

    public static void MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        MapArguments( source, buffer, arguments.Select( kv => KeyValuePair.Create( StringSlice.Create( kv.Key ), kv.Value ) ) );
    }

    public static void MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        params KeyValuePair<string, TArg?>[] arguments)
    {
        source.MapArguments( buffer, arguments.AsEnumerable() );
    }

    public static void MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<ReadOnlyMemory<char>, TArg?>> arguments)
    {
        MapArguments( source, buffer, arguments.Select( kv => KeyValuePair.Create( StringSlice.Create( kv.Key ), kv.Value ) ) );
    }

    public static void MapArguments<TArg, TResult>(
        this IMathExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        params KeyValuePair<ReadOnlyMemory<char>, TArg?>[] arguments)
    {
        source.MapArguments( buffer, arguments.AsEnumerable() );
    }

    private static void MapArguments<TArg, TResult>(
        IMathExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<StringSlice, TArg?>> arguments)
    {
        var argumentCount = source.GetArgumentCount();
        if ( argumentCount == 0 )
            return;

        if ( buffer.Length < argumentCount )
            throw new MathExpressionArgumentBufferTooSmallException( buffer.Length, argumentCount, nameof( buffer ) );

        var invalidArgumentNames = Chain<ReadOnlyMemory<char>>.Empty;

        foreach ( var (name, value) in arguments )
        {
            var nameMemory = name.AsMemory();
            var index = source.GetArgumentIndex( nameMemory );
            if ( index < 0 )
            {
                invalidArgumentNames = invalidArgumentNames.Extend( nameMemory );
                continue;
            }

            buffer[index] = value;
        }

        if ( invalidArgumentNames.Count > 0 )
            throw new InvalidMathExpressionArgumentsException( invalidArgumentNames, nameof( arguments ) );
    }
}
