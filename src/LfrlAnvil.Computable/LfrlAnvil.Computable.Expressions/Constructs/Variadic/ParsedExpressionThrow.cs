using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionThrow : ParsedExpressionVariadicFunction
{
    private readonly ConstructorInfo _exceptionCtor;

    public ParsedExpressionThrow()
    {
        _exceptionCtor = MemberInfoLocator.FindInvocationExceptionCtor();
    }

    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        var exception = CreateException( parameters );
        var result = Expression.Throw( exception );
        return result;
    }

    [Pure]
    private Expression CreateException(IReadOnlyList<Expression> parameters)
    {
        if ( parameters.Count == 0 )
            return CreateDefaultException();

        if ( parameters.Count == 1 && parameters[0].Type.IsAssignableTo( typeof( Exception ) ) )
            return parameters[0];

        return CreateFormattedException( parameters );
    }

    [Pure]
    private Expression CreateDefaultException()
    {
        return Expression.New(
            _exceptionCtor,
            Expression.Constant( Resources.InvocationHasThrownAnException ),
            Expression.Constant( Array.Empty<object?>() ) );
    }

    [Pure]
    private Expression CreateFormattedException(IReadOnlyList<Expression> parameters)
    {
        var format = parameters[0];
        Ensure.Equals( format.Type, typeof( string ), EqualityComparer<Type>.Default );

        if ( parameters.Count == 1 )
            return Expression.New( _exceptionCtor, format, Expression.Constant( Array.Empty<object?>() ) );

        if ( parameters.Skip( 1 ).All( static p => p is ConstantExpression ) )
        {
            var args = new object?[parameters.Count - 1];
            for ( var i = 1; i < parameters.Count; ++i )
                args[i - 1] = ReinterpretCast.To<ConstantExpression>( parameters[i] ).Value;

            return Expression.New( _exceptionCtor, format, Expression.Constant( args ) );
        }

        var newArgs = Expression.NewArrayInit(
            typeof( object ),
            parameters.Skip( 1 ).Select( static p => p.Type == typeof( object ) ? p : Expression.Convert( p, typeof( object ) ) ) );

        return Expression.New( _exceptionCtor, format, newArgs );
    }
}
