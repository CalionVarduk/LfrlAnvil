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

/// <summary>
/// Contains <see cref="Expression"/> extension methods.
/// </summary>
public static class ExpressionExtensions
{
    /// <summary>
    /// Extracts member's name from the provided lambda expression.
    /// </summary>
    /// <param name="source">Source lambda expression.</param>
    /// <typeparam name="T">Source type.</typeparam>
    /// <typeparam name="TMember">Source member's type.</typeparam>
    /// <returns>Name of the source's type member extracted from the provided lambda expression.</returns>
    /// <exception cref="ArgumentException">
    /// When <paramref name="source"/> <see cref="LambdaExpression.Body"/> is not an instance of <see cref="MemberExpression"/> type
    /// or when body's <see cref="MemberExpression.Expression"/> does not equal to the lambda expression's parameter.
    /// </exception>
    [Pure]
    public static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> source)
    {
        var body = source.Body;

        Ensure.IsInstanceOfType<MemberExpression>( body );
        var memberExpr = ReinterpretCast.To<MemberExpression>( body );
        Ensure.True( memberExpr.Expression == source.Parameters[0] );

        return memberExpr.Member.Name;
    }

    /// <summary>
    /// Attempts to extract typed <see cref="ConstantExpression.Value"/> from the provided <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <param name="result"><b>out</b> parameter that is set to the extracted value, if it is an instance of the provided type.</param>
    /// <typeparam name="T">Expected value type.</typeparam>
    /// <returns><b>true</b> when value is an instance of the provided type, otherwise <b>false</b>.</returns>
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

    /// <summary>
    /// Attempts to extract typed <see cref="ConstantExpression.Value"/> from the provided <paramref name="expression"/>.
    /// </summary>
    /// <param name="expression">Source constant expression.</param>
    /// <typeparam name="T">Expected value type.</typeparam>
    /// <returns>Extracted value, if it is an instance of the provided type, otherwise default value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? GetValueOrDefault<T>(this ConstantExpression expression)
    {
        return expression.Value is T value ? value : default;
    }

    /// <summary>
    /// Converts the provided <paramref name="expression"/> to an expression
    /// whose <see cref="Expression.Type"/> is equal to the expected type..
    /// </summary>
    /// <param name="expression">Source expression.</param>
    /// <typeparam name="T">Expected expression type.</typeparam>
    /// <returns>
    /// Provided <paramref name="expression"/> if its type is equal to the expected type,
    /// otherwise a conversion <see cref="UnaryExpression"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression GetOrConvert<T>(this Expression expression)
    {
        return expression.GetOrConvert( typeof( T ) );
    }

    /// <summary>
    /// Converts the provided <paramref name="expression"/> to an expression
    /// whose <see cref="Expression.Type"/> is equal to <paramref name="expectedType"/>.
    /// </summary>
    /// <param name="expression">Source expression.</param>
    /// <param name="expectedType">Expected expression type.</param>
    /// <returns>
    /// Provided <paramref name="expression"/> if its type is equal to <paramref name="expectedType"/>,
    /// otherwise a conversion <see cref="UnaryExpression"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression GetOrConvert(this Expression expression, Type expectedType)
    {
        return expression.Type == expectedType ? expression : Expression.Convert( expression, expectedType );
    }

    /// <summary>
    /// Creates a new <see cref="BinaryExpression"/> that checks if the provided <paramref name="expression"/> is null by reference.
    /// </summary>
    /// <param name="expression">Source expression.</param>
    /// <returns>New <see cref="BinaryExpression"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BinaryExpression IsNullReference(this Expression expression)
    {
        return Expression.ReferenceEqual( expression, Expression.Constant( null, expression.Type ) );
    }

    /// <summary>
    /// Creates a new <see cref="BinaryExpression"/> that checks if the provided <paramref name="expression"/> is not null by reference.
    /// </summary>
    /// <param name="expression">Source expression.</param>
    /// <returns>New <see cref="BinaryExpression"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static BinaryExpression IsNotNullReference(this Expression expression)
    {
        return Expression.ReferenceNotEqual( expression, Expression.Constant( null, expression.Type ) );
    }

    /// <summary>
    /// Creates a new <see cref="ForEachLoopExpressionCreator"/> instance.
    /// </summary>
    /// <param name="enumerable">Source enumerable expression.</param>
    /// <param name="enumeratorVariableName">Optional enumerator variable name. Equal to "enumerator" by default.</param>
    /// <param name="currentVariableName">Optional enumerator's current variable name. Equal to "current" by default.</param>
    /// <returns>New <see cref="ForEachLoopExpressionCreator"/> instance.</returns>
    /// <exception cref="ArgumentNullException">When any of the required enumerator elements are null.</exception>
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
                        static m => ! m.IsGenericMethod
                            && m.ReturnType != typeof( void )
                            && m.Name == nameof( IEnumerable.GetEnumerator )
                            && m.GetParameters().Length == 0 ) );

        Ensure.IsNotNull( getEnumeratorMethod );
        Assume.IsNotNull( getEnumeratorMethod.DeclaringType );

        var enumerator = Expression.Variable( getEnumeratorMethod.ReturnType, enumeratorVariableName );
        var enumeratorAssignment = Expression.Assign(
            enumerator,
            Expression.Call( enumerable.GetOrConvert( getEnumeratorMethod.DeclaringType ), getEnumeratorMethod ) );

        var currentProperty = enumerator.Type.FindMember(
            static t => t.GetProperties( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault(
                    static p => p.GetGetMethod() is not null
                        && p.Name == nameof( IEnumerator.Current )
                        && p.GetIndexParameters().Length == 0 ) );

        Ensure.IsNotNull( currentProperty );
        Assume.IsNotNull( currentProperty.DeclaringType );

        var moveNextMethod = enumerator.Type.FindMember(
            static t => t.GetMethods( BindingFlags.Public | BindingFlags.Instance )
                .FirstOrDefault(
                    static m => ! m.IsGenericMethod
                        && m.ReturnType == typeof( bool )
                        && m.Name == nameof( IEnumerator.MoveNext )
                        && m.GetParameters().Length == 0 ) );

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

    /// <summary>
    /// Rewrites the provided <paramref name="expression"/> by replacing <see cref="ParameterExpression"/> nodes by name.
    /// </summary>
    /// <param name="expression">Expression to rewrite.</param>
    /// <param name="parametersToReplace">Collection of (parameter-name, replacement-node) entries.</param>
    /// <returns>Rewritten <paramref name="expression"/>.</returns>
    /// <remarks>See <see cref="ExpressionParameterByNameReplacer"/> for more information.</remarks>
    [Pure]
    public static Expression ReplaceParametersByName(
        this Expression expression,
        IReadOnlyDictionary<string, Expression> parametersToReplace)
    {
        var replacer = new ExpressionParameterByNameReplacer( parametersToReplace );
        var result = replacer.Visit( expression );
        return result;
    }

    /// <summary>
    /// Rewrites the provided <paramref name="expression"/> by replacing <paramref name="parameterToReplace"/>
    /// node with <paramref name="replacement"/> node.
    /// </summary>
    /// <param name="expression">Expression to rewrite.</param>
    /// <param name="parameterToReplace"><see cref="ParameterExpression"/> node to replace.</param>
    /// <param name="replacement">Replacement <see cref="Expression"/> node.</param>
    /// <returns>Rewritten <paramref name="expression"/>.</returns>
    /// <remarks>See <see cref="ExpressionParameterReplacer"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Expression ReplaceParameter(this Expression expression, ParameterExpression parameterToReplace, Expression replacement)
    {
        return expression.ReplaceParameters( new[] { parameterToReplace }, new[] { replacement } );
    }

    /// <summary>
    /// Rewrites the provided <paramref name="expression"/> by replacing <see cref="ParameterExpression"/> nodes by position.
    /// </summary>
    /// <param name="expression">Expression to rewrite.</param>
    /// <param name="parametersToReplace">Collection of <see cref="ParameterExpression"/> nodes to replace.</param>
    /// <param name="replacements">Collection of replacement <see cref="Expression"/> nodes.</param>
    /// <returns>Rewritten <paramref name="expression"/>.</returns>
    /// <remarks>See <see cref="ExpressionParameterReplacer"/> for more information.</remarks>
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

    /// <summary>
    /// A lightweight creator of an <see cref="Expression"/> node equivalent to a foreach loop.
    /// </summary>
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

        /// <summary>
        /// <see cref="ParameterExpression"/> representing a local variable
        /// that can have the <see cref="IEnumerator.Current"/> value assigned to it.
        /// </summary>
        public ParameterExpression Current { get; }

        /// <summary>
        /// <see cref="BinaryExpression"/> representing an assignment of the <see cref="IEnumerator.Current"/> value
        /// to the <see cref="Current"/> local variable.
        /// </summary>
        public BinaryExpression CurrentAssignment { get; }

        /// <summary>
        /// <see cref="PropertyInfo"/> instance representing enumerator's <see cref="IEnumerator.Current"/> property.
        /// </summary>
        public PropertyInfo CurrentProperty { get; }

        /// <summary>
        /// Creates a new <see cref="BlockExpression"/> that represents a foreach loop.
        /// </summary>
        /// <param name="body">Loop's body expression.</param>
        /// <param name="loopBreakLabelName">
        /// Optional name of the <see cref="LabelTarget"/> that represents the loop's break. Equal to "ForEachEnd" by default.
        /// </param>
        /// <returns>New <see cref="BlockExpression"/> instance.</returns>
        /// <remarks>Use <see cref="Current"/> and <see cref="CurrentAssignment"/> properties to construct the desired loop body.</remarks>
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
