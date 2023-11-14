using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Expressions;

namespace LfrlAnvil.Extensions;

public static class ExpressionExtensions
{
    [Pure]
    public static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> source)
    {
        var body = source.Body;

        Ensure.IsInstanceOfType<MemberExpression>( body );
        var memberExpr = ReinterpretCast.To<MemberExpression>( body );
        Ensure.True( memberExpr.Expression == source.Parameters[0] );

        return memberExpr.Member.Name;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryGetValue<T>(this ConstantExpression expression, [MaybeNullWhen( false )] out T result)
    {
        if ( expression.Value is T value )
        {
            result = value;
            return true;
        }

        result = default;
        return false;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? GetValueOrDefault<T>(this ConstantExpression expression)
    {
        return expression.Value is T value ? value : default;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression GetOrConvert<T>(this Expression expression)
    {
        return expression.GetOrConvert( typeof( T ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression GetOrConvert(this Expression expression, Type expectedType)
    {
        return expression.Type == expectedType ? expression : Expression.Convert( expression, expectedType );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BinaryExpression IsNullReference(this Expression expression)
    {
        return Expression.ReferenceEqual( expression, Expression.Constant( null, expression.Type ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BinaryExpression IsNotNullReference(this Expression expression)
    {
        return Expression.ReferenceNotEqual( expression, Expression.Constant( null, expression.Type ) );
    }

    [Pure]
    public static ForEachLoopExpressionCreator ToForEachLoop(
        this Expression enumerable,
        string? enumeratorVariableName = "enumerator",
        string? currentVariableName = "current")
    {
        if ( enumerable.Type.IsArray )
        {
            var elementType = enumerable.Type.GetElementType();
            if ( elementType is not null )
            {
                var arraySegmentType = typeof( ArraySegment<> ).MakeGenericType( elementType );
                var arraySegmentCtor = arraySegmentType.GetConstructor( new[] { enumerable.Type } );
                Assume.IsNotNull( arraySegmentCtor );
                enumerable = Expression.New( arraySegmentCtor, enumerable );
            }
        }

        var getEnumeratorMethod = enumerable.Type.FindMember(
            static t =>
                t.GetMethods( BindingFlags.Public | BindingFlags.Instance )
                    .FirstOrDefault(
                        static m => ! m.IsGenericMethod &&
                            m.ReturnType != typeof( void ) &&
                            m.Name == nameof( IEnumerable.GetEnumerator ) &&
                            m.GetParameters().Length == 0 ) );

        Ensure.IsNotNull( getEnumeratorMethod );
        Assume.IsNotNull( getEnumeratorMethod.DeclaringType );

        var enumerator = Expression.Variable( getEnumeratorMethod.ReturnType, enumeratorVariableName );
        var enumeratorAssignment = Expression.Assign(
            enumerator,
            Expression.Call( enumerable.GetOrConvert( getEnumeratorMethod.DeclaringType ), getEnumeratorMethod ) );

        var currentProperty = enumerator.Type.FindMember(
            static t => t.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault(
                    static p => p.GetGetMethod() is not null &&
                        p.Name == nameof( IEnumerator.Current ) &&
                        p.GetIndexParameters().Length == 0 ) );

        Ensure.IsNotNull( currentProperty );
        Assume.IsNotNull( currentProperty.DeclaringType );

        var moveNextMethod = enumerator.Type.FindMember(
            static t => t.GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault(
                    static m => ! m.IsGenericMethod &&
                        m.ReturnType == typeof( bool ) &&
                        m.Name == nameof( IEnumerator.MoveNext ) &&
                        m.GetParameters().Length == 0 ) );

        Ensure.IsNotNull( moveNextMethod );
        Assume.IsNotNull( moveNextMethod.DeclaringType );

        var disposeMethod = enumerator.Type.FindMember(
            static t => t.GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault(
                    static m => ! m.IsGenericMethod && m.Name == nameof( IDisposable.Dispose ) && m.GetParameters().Length == 0 ) );

        var current = Expression.Variable( currentProperty.PropertyType, currentVariableName );
        var currentMemberAccess = Expression.MakeMemberAccess( enumerator.GetOrConvert( currentProperty.DeclaringType ), currentProperty );
        var currentAssignment = Expression.Assign( current, currentMemberAccess );
        var moveNextCall = Expression.Call( enumerator.GetOrConvert( moveNextMethod.DeclaringType ), moveNextMethod );

        MethodCallExpression? disposeCall = null;
        if ( disposeMethod is not null )
        {
            Assume.IsNotNull( disposeMethod.DeclaringType );
            disposeCall = Expression.Call( enumerator.GetOrConvert( disposeMethod.DeclaringType ), disposeMethod );
        }

        return new ForEachLoopExpressionCreator(
            current,
            currentAssignment,
            currentProperty,
            enumerator,
            enumeratorAssignment,
            moveNextCall,
            disposeCall );
    }

    [Pure]
    public static Expression ReplaceParametersByName(
        this Expression expression,
        IReadOnlyDictionary<string, Expression> parametersToReplace)
    {
        var replacer = new ExpressionParameterByNameReplacer( parametersToReplace );
        var result = replacer.Visit( expression );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression parameterToReplace, Expression replacement)
    {
        return expression.ReplaceParameters( new[] { parameterToReplace }, new[] { replacement } );
    }

    [Pure]
    public static Expression ReplaceParameters(
        this Expression expression,
        ParameterExpression[] parametersToReplace,
        Expression[] replacements)
    {
        var replacer = new ExpressionParameterReplacer( parametersToReplace, replacements );
        var result = replacer.Visit( expression );
        return result;
    }

    public readonly struct ForEachLoopExpressionCreator
    {
        private readonly BinaryExpression _enumeratorAssignment;
        private readonly MethodCallExpression _moveNextCall;
        private readonly MethodCallExpression? _disposeCall;
        private readonly ParameterExpression[] _blockVariables;

        internal ForEachLoopExpressionCreator(
            ParameterExpression current,
            BinaryExpression currentAssignment,
            PropertyInfo currentProperty,
            ParameterExpression enumerator,
            BinaryExpression enumeratorAssignment,
            MethodCallExpression moveNextCall,
            MethodCallExpression? disposeCall)
        {
            Current = current;
            CurrentAssignment = currentAssignment;
            CurrentProperty = currentProperty;
            _enumeratorAssignment = enumeratorAssignment;
            _moveNextCall = moveNextCall;
            _disposeCall = disposeCall;
            _blockVariables = new[] { enumerator, current };
        }

        public ParameterExpression Current { get; }
        public BinaryExpression CurrentAssignment { get; }
        public PropertyInfo CurrentProperty { get; }

        [Pure]
        public BlockExpression Create(Expression body, string? loopBreakLabelName = "ForEachEnd")
        {
            var loopBreakLabel = Expression.Label( loopBreakLabelName );
            var loopBreak = Expression.Break( loopBreakLabel );
            var loop = Expression.Loop( Expression.IfThenElse( _moveNextCall, body, loopBreak ), loopBreakLabel );

            if ( _disposeCall is null )
                return Expression.Block( _blockVariables, _enumeratorAssignment, loop );

            var tryBlock = Expression.Block( _enumeratorAssignment, loop );
            var enumerator = _blockVariables[0];
            Expression finallyBlock = enumerator.Type.IsValueType
                ? _disposeCall
                : Expression.IfThen( enumerator.IsNotNullReference(), _disposeCall );

            return Expression.Block( _blockVariables, Expression.TryFinally( tryBlock, finallyBlock ) );
        }
    }
}
