using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mathematical.Expressions.Exceptions;

namespace LfrlAnvil.Mathematical.Expressions;

public sealed class MathExpressionBufferedDelegate<TArg, TResult>
{
    private readonly TArg?[] _buffer;

    internal MathExpressionBufferedDelegate(IMathExpressionDelegate<TArg, TResult> @base)
    {
        Base = @base;
        var argumentCount = Base.GetArgumentCount();
        _buffer = argumentCount == 0 ? Array.Empty<TArg?>() : new TArg?[argumentCount];
    }

    public IMathExpressionDelegate<TArg, TResult> Base { get; }

    public MathExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(int index, TArg? value)
    {
        _buffer[index] = value;
        return this;
    }

    public MathExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(string argumentName, TArg? value)
    {
        var index = Base.GetArgumentIndex( argumentName );
        if ( index < 0 )
            throw new InvalidMathExpressionArgumentsException( Chain.Create( argumentName.AsMemory() ), nameof( argumentName ) );

        _buffer[index] = value;
        return this;
    }

    public MathExpressionBufferedDelegate<TArg, TResult> SetArgumentValue(ReadOnlyMemory<char> argumentName, TArg? value)
    {
        var index = Base.GetArgumentIndex( argumentName );
        if ( index < 0 )
            throw new InvalidMathExpressionArgumentsException( Chain.Create( argumentName ), nameof( argumentName ) );

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
        var index = Base.GetArgumentIndex( argumentName );
        if ( index < 0 )
            throw new InvalidMathExpressionArgumentsException( Chain.Create( argumentName.AsMemory() ), nameof( argumentName ) );

        return GetArgumentValue( index );
    }

    [Pure]
    public TArg? GetArgumentValue(ReadOnlyMemory<char> argumentName)
    {
        var index = Base.GetArgumentIndex( argumentName );
        if ( index < 0 )
            throw new InvalidMathExpressionArgumentsException( Chain.Create( argumentName ), nameof( argumentName ) );

        return GetArgumentValue( index );
    }

    public MathExpressionBufferedDelegate<TArg, TResult> ClearArgumentValues(TArg? defaultValue = default)
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
