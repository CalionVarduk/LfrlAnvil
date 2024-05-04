using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

/// <summary>
/// Contains <see cref="IParsedExpressionDelegate{TArg,TResult}"/> extension methods.
/// </summary>
public static class ParsedExpressionDelegateExtensions
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBufferedDelegate{TArg,TResult}"/> instance.
    /// </summary>
    /// <param name="source">Underlying delegate.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="ParsedExpressionBufferedDelegate{TArg,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionBufferedDelegate<TArg, TResult> ToBuffered<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source)
    {
        return new ParsedExpressionBufferedDelegate<TArg, TResult>( source );
    }

    /// <summary>
    /// Creates a new buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
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

    /// <summary>
    /// Creates a new buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        params KeyValuePair<string, TArg?>[] arguments)
    {
        return source.MapArguments( arguments.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        IEnumerable<KeyValuePair<StringSegment, TArg?>> arguments)
    {
        var argumentCount = source.Arguments.Count;
        var buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
        source.MapArguments( buffer, arguments );
        return buffer;
    }

    /// <summary>
    /// Creates a new buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    [Pure]
    public static TArg?[] MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        params KeyValuePair<StringSegment, TArg?>[] arguments)
    {
        return source.MapArguments( arguments.AsEnumerable() );
    }

    /// <summary>
    /// Populates the provided buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="buffer">Target buffer of argument values.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    /// <exception cref="ParsedExpressionArgumentBufferTooSmallException">
    /// When the provided <paramref name="buffer"/> is too small.
    /// </exception>
    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<string, TArg?>> arguments)
    {
        MapArguments( source, buffer, arguments.Select( static kv => KeyValuePair.Create( ( StringSegment )kv.Key, kv.Value ) ) );
    }

    /// <summary>
    /// Populates the provided buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="buffer">Target buffer of argument values.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    /// <exception cref="ParsedExpressionArgumentBufferTooSmallException">
    /// When the provided <paramref name="buffer"/> is too small.
    /// </exception>
    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        params KeyValuePair<string, TArg?>[] arguments)
    {
        source.MapArguments( buffer, arguments.AsEnumerable() );
    }

    /// <summary>
    /// Populates the provided buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="buffer">Target buffer of argument values.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    /// <exception cref="ParsedExpressionArgumentBufferTooSmallException">
    /// When the provided <paramref name="buffer"/> is too small.
    /// </exception>
    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        IEnumerable<KeyValuePair<StringSegment, TArg?>> arguments)
    {
        var argumentCount = source.Arguments.Count;
        if ( argumentCount == 0 )
            return;

        if ( buffer.Length < argumentCount )
            throw new ParsedExpressionArgumentBufferTooSmallException( buffer.Length, argumentCount, nameof( buffer ) );

        var invalidArgumentNames = Chain<StringSegment>.Empty;

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

    /// <summary>
    /// Populates the provided buffer of argument values for the provided delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <param name="buffer">Target buffer of argument values.</param>
    /// <param name="arguments">Collection of (name, value) pairs that represents arguments and values to set for them.</param>
    /// <typeparam name="TArg">Argument type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New buffer of argument values.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When at least one argument does not exist.</exception>
    /// <exception cref="ParsedExpressionArgumentBufferTooSmallException">
    /// When the provided <paramref name="buffer"/> is too small.
    /// </exception>
    public static void MapArguments<TArg, TResult>(
        this IParsedExpressionDelegate<TArg, TResult> source,
        TArg?[] buffer,
        params KeyValuePair<StringSegment, TArg?>[] arguments)
    {
        source.MapArguments( buffer, arguments.AsEnumerable() );
    }
}
