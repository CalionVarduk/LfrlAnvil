using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

/// <summary>
/// Represents a function construct.
/// </summary>
public class ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <param name="inlineIfPossible">
    /// Specifies whether or not the <see cref="Lambda"/> should be inlined if possible. Equal to <b>true</b> by default.
    /// </param>
    /// <exception cref="ArgumentException">When <paramref name="lambda"/> return type is equal to <b>void</b>.</exception>
    public ParsedExpressionFunction(LambdaExpression lambda, bool inlineIfPossible = true)
    {
        Ensure.NotEquals( lambda.ReturnType, typeof( void ), EqualityComparer<Type>.Default );
        IsInlined = inlineIfPossible && lambda.Parameters.All( static p => p.Name is not null );
        Lambda = lambda;
    }

    /// <summary>
    /// Underlying <see cref="LambdaExpression"/>.
    /// </summary>
    public LambdaExpression Lambda { get; }

    /// <summary>
    /// Specifies whether or not the <see cref="Lambda"/> will be inlined.
    /// </summary>
    public bool IsInlined { get; }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<TReturn> Create<TReturn>(Expression<Func<TReturn>> lambda)
    {
        return new ParsedExpressionFunction<TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, TReturn> Create<T1, TReturn>(Expression<Func<T1, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,T2,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, TReturn> Create<T1, T2, TReturn>(Expression<Func<T1, T2, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,T2,T3,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, TReturn> Create<T1, T2, T3, TReturn>(Expression<Func<T1, T2, T3, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,T2,T3,T4,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, TReturn> Create<T1, T2, T3, T4, TReturn>(
        Expression<Func<T1, T2, T3, T4, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, T5, TReturn> Create<T1, T2, T3, T4, T5, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, T5, TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,T6,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,T6,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, TReturn> Create<T1, T2, T3, T4, T5, T6, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, TReturn>( lambda );
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,T6,T7,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    /// <returns>New <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,T6,T7,TResult}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, T7, TReturn> Create<T1, T2, T3, T4, T5, T6, T7, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, T6, T7, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, T7, TReturn>( lambda );
    }

    [Pure]
    internal Expression Process(IReadOnlyList<Expression> parameters)
    {
        Assume.Equals( parameters.Count, Lambda.Parameters.Count );

        if ( ! IsInlined )
            return Expression.Invoke( Lambda, parameters );

        if ( parameters.Count == 0 )
            return Lambda.Body;

        var parametersToReplace = new Dictionary<string, Expression>();
        foreach ( var (parameter, value) in Lambda.Parameters.Zip( parameters ) )
        {
            Assume.IsNotNull( parameter.Name );
            parametersToReplace.Add( parameter.Name, value );
        }

        var result = Lambda.Body.ReplaceParametersByName( parametersToReplace );
        return result;
    }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="T2">Second parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, T2, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, T2, TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="T2">Second parameter's type.</typeparam>
/// <typeparam name="T3">Third parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, T2, T3, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="T2">Second parameter's type.</typeparam>
/// <typeparam name="T3">Third parameter's type.</typeparam>
/// <typeparam name="T4">Fourth parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, T2, T3, T4, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="T2">Second parameter's type.</typeparam>
/// <typeparam name="T3">Third parameter's type.</typeparam>
/// <typeparam name="T4">Fourth parameter's type.</typeparam>
/// <typeparam name="T5">Fifth parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, T2, T3, T4, T5, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, T5, TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="T2">Second parameter's type.</typeparam>
/// <typeparam name="T3">Third parameter's type.</typeparam>
/// <typeparam name="T4">Fourth parameter's type.</typeparam>
/// <typeparam name="T5">Fifth parameter's type.</typeparam>
/// <typeparam name="T6">Sixth parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,T6,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> lambda)
        : base( lambda ) { }
}

/// <summary>
/// Represents a function construct.
/// </summary>
/// <typeparam name="T1">First parameter's type.</typeparam>
/// <typeparam name="T2">Second parameter's type.</typeparam>
/// <typeparam name="T3">Third parameter's type.</typeparam>
/// <typeparam name="T4">Fourth parameter's type.</typeparam>
/// <typeparam name="T5">Fifth parameter's type.</typeparam>
/// <typeparam name="T6">Sixth parameter's type.</typeparam>
/// <typeparam name="T7">Seventh parameter's type.</typeparam>
/// <typeparam name="TResult">result type.</typeparam>
public class ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, T7, TResult> : ParsedExpressionFunction
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionFunction{T1,T2,T3,T4,T5,T6,T7,TResult}"/> instance.
    /// </summary>
    /// <param name="lambda">Underlying <see cref="LambdaExpression"/>.</param>
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> lambda)
        : base( lambda ) { }
}
