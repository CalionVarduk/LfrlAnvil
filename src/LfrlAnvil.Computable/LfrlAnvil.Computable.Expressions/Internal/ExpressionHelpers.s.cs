using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal static class ExpressionHelpers
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression TryUpdateThrowType(Expression expression, Type expectedType)
    {
        if ( expression.NodeType != ExpressionType.Throw ||
            expression.Type == expectedType ||
            expression is not UnaryExpression throwExpression )
            return expression;

        return Expression.Throw( throwExpression.Operand, expectedType );
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
            setItem.Invoke( result, new[] { i, ReinterpretCast.To<ConstantExpression>( elements[i] ).Value } );

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

        var property = ReinterpretCast.To<PropertyInfo>( member );
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

        var property = ReinterpretCast.To<PropertyInfo>( indexer );
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
    internal static ConstantExpression CreateConstantDelegateInvocation(
        ConstantExpression operand,
        IReadOnlyList<Expression> parameters)
    {
        if ( ! operand.Type.IsAssignableTo( typeof( Delegate ) ) )
            throw new ParsedExpressionNonInvocableTypeExpression( operand.Type );

        if ( operand.Value is null )
            throw new TargetException( ExceptionResources.NonStaticMethodRequiresTarget );

        var @delegate = DynamicCast.To<Delegate>( operand.Value );
        var method = @delegate.GetType().GetMethod( nameof( Action.Invoke ) )!;
        var @params = parameters.GetConstantValues();
        return Expression.Constant( @delegate.DynamicInvoke( @params ), method.ReturnType );
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
        if ( expression is null || argumentCount == 0 )
            return false;

        if ( expression.NodeType != ExpressionType.ArrayIndex || expression is not BinaryExpression arrayIndexExpression )
            return false;

        if ( ! ReferenceEquals( arrayIndexExpression.Left, parameter ) )
            return false;

        if ( arrayIndexExpression.Right is not ConstantExpression indexExpression || indexExpression.Type != typeof( int ) )
            return false;

        index = DynamicCast.Unbox<int>( indexExpression.Value );
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
            return Type.EmptyTypes;

        var result = new Type[expressions.Count];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = expressions[i].Type;

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetConstantMemberNameValue(this Expression expression)
    {
        return GetConstantRefValue<string>( expression, Resources.MemberNameMustBeConstantNonNullString );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Type GetConstantArrayElementTypeValue(this Expression expression)
    {
        return GetConstantRefValue<Type>( expression, Resources.ArrayElementTypeMustBeConstantNonNullType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Expression CreateLambdaPlaceholder(Type type)
    {
        return new LambdaPlaceholderExpression( type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool IsLambdaPlaceholder([NotNullWhen( true )] Expression? expression)
    {
        return expression is LambdaPlaceholderExpression;
    }

    [Pure]
    internal static Expression IncludeVariables(Expression expression, IReadOnlyList<VariableAssignment> assignments)
    {
        if ( assignments.Count == 0 )
            return expression;

        var variables = assignments.Select( a => a.Variable ).DistinctBy( v => v.Name );
        var expressions = assignments.Select( a => a.Expression ).Append( expression );
        var result = Expression.Block( variables, expressions );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static object?[] GetConstantValues(this IReadOnlyList<Expression> expressions)
    {
        if ( expressions.Count == 0 )
            return Array.Empty<object?>();

        var result = new object?[expressions.Count];
        for ( var i = 0; i < result.Length; ++i )
            result[i] = ReinterpretCast.To<ConstantExpression>( expressions[i] ).Value;

        return result;
    }

    [Pure]
    private static T GetConstantRefValue<T>(this Expression expression, string errorMessage)
        where T : class
    {
        if ( expression is not ConstantExpression constant || ! expression.Type.IsAssignableTo( typeof( T ) ) )
            throw new ParsedExpressionInvalidExpressionException( errorMessage, expression );

        var result = DynamicCast.To<T>( constant.Value );
        if ( result is null )
            throw new ParsedExpressionInvalidExpressionException( errorMessage, expression );

        return result;
    }

    private sealed class LambdaPlaceholderExpression : Expression
    {
        internal LambdaPlaceholderExpression(Type type)
        {
            Type = type;
        }

        public override Type Type { get; }
        public override ExpressionType NodeType => ExpressionType.Extension;

        [Pure]
        public override string ToString()
        {
            return $"LambdaPlaceholder({Type.GetDebugString()})";
        }

        [Pure]
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }
    }
}
