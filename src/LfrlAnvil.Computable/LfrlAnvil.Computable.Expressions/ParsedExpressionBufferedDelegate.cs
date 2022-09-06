using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions;

public sealed class ParsedExpressionBufferedDelegate<TArg, TResult>
{
    private readonly TArg?[] _buffer;

    internal ParsedExpressionBufferedDelegate(IParsedExpressionDelegate<TArg, TResult> @base)
    {
        Base = @base;
        var argumentCount = Base.Arguments.Count;
        _buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
    }

    public IParsedExpressionDelegate<TArg, TResult> Base { get; }

    public ParsedExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(int index, TArg? value)
    {
        _buffer[index] = value;
        return this;
    }

    public ParsedExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(string argumentName, TArg? value)
    {
        return SetArgumentValue( argumentName.AsSlice(), value );
    }

    public ParsedExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(StringSlice argumentName, TArg? value)
    {
        var index = Base.Arguments.GetIndex( argumentName );
        if ( index < 0 )
            throw new InvalidParsedExpressionArgumentsException( Chain.Create( argumentName ), nameof( argumentName ) );

        _buffer[index] = value;
        return this;
    }

    [Pure]
    public TArg? GetArgumentValue(int index)
    {
        return _buffer[index];
    }

    [Pure]
    public TArg? GetArgumentValue(string argumentName)
    {
        return GetArgumentValue( argumentName.AsSlice() );
    }

    [Pure]
    public TArg? GetArgumentValue(StringSlice argumentName)
    {
        var index = Base.Arguments.GetIndex( argumentName );
        if ( index < 0 )
            throw new InvalidParsedExpressionArgumentsException( Chain.Create( argumentName ), nameof( argumentName ) );

        return GetArgumentValue( index );
    }

    public ParsedExpressionBufferedDelegate<TArg, TResult> ClearArgumentValues(TArg? defaultValue = default)
    {
        Array.Fill( _buffer, defaultValue );
        return this;
    }

    [Pure]
    public TResult Invoke()
    {
        return Base.Delegate( _buffer );
    }
}
