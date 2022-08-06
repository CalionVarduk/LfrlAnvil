using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Mathematical.Expressions.Exceptions;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Constructs;

public class MathExpressionTypeConverter : IMathExpressionConstruct
{
    public MathExpressionTypeConverter(Type targetType, Type? sourceType = null)
    {
        TargetType = targetType;
        SourceType = sourceType;
    }

    public Type TargetType { get; }
    public Type? SourceType { get; }

    public void Process(MathExpressionOperandStack operandStack)
    {
        Debug.Assert( operandStack.Count > 0, "operand stack cannot be empty" );

        var operand = operandStack.Pop();
        var result = CreateResult( operand );

        if ( result.Type != TargetType )
        {
            if ( ! result.Type.IsAssignableTo( TargetType ) )
                throw new MathExpressionTypeConverterException( Resources.InvalidTypeConverterResultType( result.Type, TargetType ), this );

            result = Expression.Convert( result, TargetType );
        }

        operandStack.Push( result );
    }

    protected virtual Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return null;
    }

    protected virtual Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Convert( operand, TargetType );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Expression CreateResult(Expression operand)
    {
        if ( operand.Type == TargetType )
            return operand;

        if ( operand.NodeType == ExpressionType.Constant )
            return TryCreateFromConstant( (ConstantExpression)operand ) ?? CreateConversionExpression( operand );

        return CreateConversionExpression( operand );
    }
}

public class MathExpressionTypeConverter<TTarget> : MathExpressionTypeConverter
{
    public MathExpressionTypeConverter(Type? sourceType = null)
        : base( typeof( TTarget ), sourceType ) { }
}

public class MathExpressionTypeConverter<TTarget, TSource> : MathExpressionTypeConverter<TTarget>
{
    public MathExpressionTypeConverter()
        : base( typeof( TSource ) ) { }

    protected static bool TryGetSourceValue(ConstantExpression expression, [MaybeNullWhen( false )] out TSource result)
    {
        return expression.TryGetValue( out result );
    }
}
