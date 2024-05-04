using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a compiled parsed expression with its own argument values buffer.
/// </summary>
/// <typeparam name="TArg">Argument type.</typeparam>
/// <typeparam name="TResult">Result type.</typeparam>
public sealed class ParsedExpressionBufferedDelegate<TArg, TResult>
{
    private readonly TArg?[] _buffer;

    internal ParsedExpressionBufferedDelegate(IParsedExpressionDelegate<TArg, TResult> @base)
    {
        Base = @base;
        var argumentCount = Base.Arguments.Count;
        _buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
    }

    /// <summary>
    /// Underlying delegate.
    /// </summary>
    public IParsedExpressionDelegate<TArg, TResult> Base { get; }

    /// <summary>
    /// Sets the value for an argument at the specified 0-based position.
    /// </summary>
    /// <param name="index">0-based position of an argument.</param>
    /// <param name="value">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is not valid.</exception>
    public ParsedExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(int index, TArg? value)
    {
        _buffer[index] = value;
        return this;
    }

    /// <summary>
    /// Sets the value for a named argument.
    /// </summary>
    /// <param name="argumentName">Argument name.</param>
    /// <param name="value">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When argument with the provided name does not exist.</exception>
    public ParsedExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(StringSegment argumentName, TArg? value)
    {
        var index = Base.Arguments.GetIndex( argumentName );
        if ( index < 0 )
            throw new InvalidParsedExpressionArgumentsException( Chain.Create( argumentName ), nameof( argumentName ) );

        _buffer[index] = value;
        return this;
    }

    /// <summary>
    /// Gets the value for an argument at the specified 0-based position.
    /// </summary>
    /// <param name="index">0-based position of an argument.</param>
    /// <returns>Current value assigned to the argument.</returns>
    /// <exception cref="IndexOutOfRangeException">When <paramref name="index"/> is not valid.</exception>
    [Pure]
    public TArg? GetArgumentValue(int index)
    {
        return _buffer[index];
    }

    /// <summary>
    /// Gets the value for a named argument.
    /// </summary>
    /// <param name="argumentName">Argument name.</param>
    /// <returns>Current value assigned to the argument.</returns>
    /// <exception cref="InvalidParsedExpressionArgumentsException">When argument with the provided name does not exist.</exception>
    [Pure]
    public TArg? GetArgumentValue(StringSegment argumentName)
    {
        var index = Base.Arguments.GetIndex( argumentName );
        if ( index < 0 )
            throw new InvalidParsedExpressionArgumentsException( Chain.Create( argumentName ), nameof( argumentName ) );

        return GetArgumentValue( index );
    }

    /// <summary>
    /// Resets all buffered argument values.
    /// </summary>
    /// <param name="defaultValue">Optional argument value.</param>
    /// <returns><b>this</b>.</returns>
    public ParsedExpressionBufferedDelegate<TArg, TResult> ClearArgumentValues(TArg? defaultValue = default)
    {
        Array.Fill( _buffer, defaultValue );
        return this;
    }

    /// <summary>
    /// Invokes this delegate with buffered argument values.
    /// </summary>
    /// <returns>Invocation result.</returns>
    [Pure]
    public TResult Invoke()
    {
        return Base.Delegate( _buffer );
    }
}
