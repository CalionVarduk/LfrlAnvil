using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

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
}
