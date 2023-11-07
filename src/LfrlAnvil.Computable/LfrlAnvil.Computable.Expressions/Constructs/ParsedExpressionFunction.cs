using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Constructs;

public class ParsedExpressionFunction
{
    public ParsedExpressionFunction(LambdaExpression lambda, bool inlineIfPossible = true)
    {
        Ensure.NotEquals( lambda.ReturnType, typeof( void ), EqualityComparer<Type>.Default );
        IsInlined = inlineIfPossible && lambda.Parameters.All( static p => p.Name is not null );
        Lambda = lambda;
    }

    public LambdaExpression Lambda { get; }
    public bool IsInlined { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<TReturn> Create<TReturn>(Expression<Func<TReturn>> lambda)
    {
        return new ParsedExpressionFunction<TReturn>( lambda );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, TReturn> Create<T1, TReturn>(Expression<Func<T1, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, TReturn>( lambda );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, TReturn> Create<T1, T2, TReturn>(Expression<Func<T1, T2, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, TReturn>( lambda );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, TReturn> Create<T1, T2, T3, TReturn>(Expression<Func<T1, T2, T3, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, TReturn>( lambda );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, TReturn> Create<T1, T2, T3, T4, TReturn>(
        Expression<Func<T1, T2, T3, T4, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, TReturn>( lambda );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, T5, TReturn> Create<T1, T2, T3, T4, T5, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, T5, TReturn>( lambda );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, TReturn> Create<T1, T2, T3, T4, T5, T6, TReturn>(
        Expression<Func<T1, T2, T3, T4, T5, T6, TReturn>> lambda)
    {
        return new ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, TReturn>( lambda );
    }

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

public class ParsedExpressionFunction<TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, T2, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, T2, TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, T2, T3, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, T2, T3, T4, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, T2, T3, T4, T5, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, T5, TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, T5, T6, TResult>> lambda)
        : base( lambda ) { }
}

public class ParsedExpressionFunction<T1, T2, T3, T4, T5, T6, T7, TResult> : ParsedExpressionFunction
{
    public ParsedExpressionFunction(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TResult>> lambda)
        : base( lambda ) { }
}
