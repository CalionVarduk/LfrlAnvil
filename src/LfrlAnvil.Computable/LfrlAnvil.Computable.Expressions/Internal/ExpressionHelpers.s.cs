using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class ExpressionHelpers
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression TryUpdateThrowType(Expression expression, Type expectedType)
    {
        if ( expression.NodeType != ExpressionType.Throw || expression.Type == expectedType )
            return expression;

        return Expression.Throw( ((UnaryExpression)expression).Operand, expectedType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression CreateConstantArray(Type elementType, IReadOnlyList<Expression> elements)
    {
        var arrayType = elementType.MakeArrayType();

        if ( elements.Count == 0 )
        {
            var emptyArray = MemberInfoLocator.FindArrayEmptyMethod( elementType );
            return Expression.Constant( emptyArray.Invoke( null, Array.Empty<object>() ), arrayType );
        }

        var arrayCtor = MemberInfoLocator.FindArrayCtor( arrayType );
        var setItem = MemberInfoLocator.FindArraySetMethod( arrayType );
        var result = arrayCtor.Invoke( new object[] { elements.Count } );

        for ( var i = 0; i < elements.Count; ++i )
            setItem.Invoke( result, new[] { i, ((ConstantExpression)elements[i]).Value } );

        return Expression.Constant( result, arrayType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression CreateVariableArray(Type elementType, IReadOnlyList<Expression> elements)
    {
        var result = Expression.NewArrayInit(
            elementType,
            elements.Select( p => p.Type == elementType ? p : Expression.Convert( p, elementType ) ) );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ConstantExpression CreateConstantMemberAccess(ConstantExpression operand, MemberInfo member)
    {
        if ( member is FieldInfo field )
            return Expression.Constant( field.GetValue( operand.Value ), field.FieldType );

        var property = (PropertyInfo)member;
        return Expression.Constant( property.GetValue( operand.Value ), property.PropertyType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ConstantExpression CreateConstantIndexer(
        ConstantExpression operand,
        MemberInfo indexer,
        IReadOnlyList<Expression> parameters)
    {
        if ( indexer is MethodInfo method )
            return CreateConstantMethodCall( operand, method, parameters );

        var property = (PropertyInfo)indexer;
        var @params = parameters.GetConstantValues();
        return Expression.Constant( property.GetValue( operand.Value, @params ), property.PropertyType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ConstantExpression CreateConstantMethodCall(
        ConstantExpression operand,
        MethodInfo method,
        IReadOnlyList<Expression> parameters)
    {
        var @params = parameters.GetConstantValues();
        return Expression.Constant( method.Invoke( operand.Value, @params ), method.ReturnType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static BinaryExpression CreateArgumentAccess(this ParameterExpression parameter, int index)
    {
        var indexExpression = Expression.Constant( index );
        var result = Expression.ArrayIndex( parameter, indexExpression );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool TryGetArgumentAccessIndex(
        [NotNullWhen( true )] this Expression? expression,
        ParameterExpression parameter,
        int argumentCount,
        out int index)
    {
        index = 0;
        if ( expression is null )
            return false;

        if ( expression.NodeType != ExpressionType.ArrayIndex )
            return false;

        var arrayIndexExpression = (BinaryExpression)expression;
        if ( ! ReferenceEquals( arrayIndexExpression.Left, parameter ) )
            return false;

        if ( arrayIndexExpression.Right.NodeType != ExpressionType.Constant )
            return false;

        var indexExpression = (ConstantExpression)arrayIndexExpression.Right;
        index = (int)indexExpression.Value!;

        return index >= 0 && index < argumentCount;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression[] Slice(this IReadOnlyList<Expression> expressions, int startIndex)
    {
        return Slice( expressions, startIndex, expressions.Count - startIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression[] Slice(this IReadOnlyList<Expression> expressions, int startIndex, int length)
    {
        Assume.IsInRange( startIndex, 0, expressions.Count, nameof( startIndex ) );
        Assume.IsInRange( length, 0, expressions.Count - startIndex, nameof( length ) );

        if ( length == 0 )
            return Array.Empty<Expression>();

        var result = new Expression[length];
        for ( var i = 0; i < length; ++i )
            result[i] = expressions[i + startIndex];

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Type[] GetExpressionTypes(this IReadOnlyList<Expression> expressions)
    {
        if ( expressions.Count == 0 )
            return Array.Empty<Type>();

        var result = new Type[expressions.Count];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = expressions[i].Type;

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static object?[] GetConstantValues(this IReadOnlyList<Expression> expressions)
    {
        if ( expressions.Count == 0 )
            return Array.Empty<object?>();

        var result = new object?[expressions.Count];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = ((ConstantExpression)expressions[i]).Value;

        return result;
    }

    [Pure]
    internal static string GetConstantMemberNameValue(this Expression expression)
    {
        if ( expression.NodeType != ExpressionType.Constant || expression.Type != typeof( string ) )
            throw new ParsedExpressionInvalidExpressionException( Resources.MemberNameMustBeConstantNonNullString, expression );

        var result = (string?)((ConstantExpression)expression).Value;
        if ( result is null )
            throw new ParsedExpressionInvalidExpressionException( Resources.MemberNameMustBeConstantNonNullString, expression );

        return result;
    }

    [Pure]
    internal static Type GetConstantArrayElementTypeValue(this Expression expression)
    {
        if ( expression.NodeType != ExpressionType.Constant || ! expression.Type.IsAssignableTo( typeof( Type ) ) )
            throw new ParsedExpressionInvalidExpressionException( Resources.ArrayElementTypeMustBeConstantNonNullType, expression );

        var result = (Type?)((ConstantExpression)expression).Value;
        if ( result is null )
            throw new ParsedExpressionInvalidExpressionException( Resources.ArrayElementTypeMustBeConstantNonNullType, expression );

        return result;
    }
}
