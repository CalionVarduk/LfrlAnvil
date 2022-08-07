using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public class ParsedExpressionConstant
{
    public ParsedExpressionConstant(Type type, object? value)
    {
        Expression = System.Linq.Expressions.Expression.Constant( value, type );
    }

    public ConstantExpression Expression { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionConstant<T> Create<T>(T value)
    {
        return new ParsedExpressionConstant<T>( value );
    }
}

public class ParsedExpressionConstant<T> : ParsedExpressionConstant
{
    public ParsedExpressionConstant(T value)
        : base( typeof( T ), value ) { }
}
