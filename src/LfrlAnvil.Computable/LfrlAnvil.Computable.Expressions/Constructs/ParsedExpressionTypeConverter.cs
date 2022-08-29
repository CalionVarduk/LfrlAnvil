using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public class ParsedExpressionTypeConverter
{
    public ParsedExpressionTypeConverter(Type targetType, Type? sourceType = null)
    {
        TargetType = targetType;
        SourceType = sourceType;
    }

    public Type TargetType { get; }
    public Type? SourceType { get; }

    [Pure]
    internal Expression Process(Expression operand)
    {
        var result = CreateResult( operand );

        if ( result.Type != TargetType )
        {
            if ( ! result.Type.IsAssignableTo( TargetType ) )
                throw new ParsedExpressionTypeConverterException(
                    Resources.InvalidTypeConverterResultType( result.Type, TargetType ),
                    this );

            result = Expression.Convert( result, TargetType );
        }

        return result;
    }

    [Pure]
    protected virtual Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return null;
    }

    [Pure]
    protected virtual Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Convert( operand, TargetType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression operand)
    {
        if ( operand.Type == TargetType )
            return operand;

        if ( operand.NodeType == ExpressionType.Constant )
            return TryCreateFromConstant( ReinterpretCast.To<ConstantExpression>( operand ) ) ?? CreateConversionExpression( operand );

        return CreateConversionExpression( operand );
    }
}

public class ParsedExpressionTypeConverter<TTarget> : ParsedExpressionTypeConverter
{
    public ParsedExpressionTypeConverter(Type? sourceType = null)
        : base( typeof( TTarget ), sourceType ) { }
}

public class ParsedExpressionTypeConverter<TTarget, TSource> : ParsedExpressionTypeConverter<TTarget>
{
    public ParsedExpressionTypeConverter()
        : base( typeof( TSource ) ) { }

    protected static bool TryGetSourceValue(ConstantExpression expression, [MaybeNullWhen( false )] out TSource result)
    {
        return expression.TryGetValue( out result );
    }
}
