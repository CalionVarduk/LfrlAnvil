using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionIndexerCall : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    public ParsedExpressionIndexerCall(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 2, nameof( parameters ) );

        var target = parameters[0];
        var callParameters = parameters.Slice( 1 );
        var parameterTypes = callParameters.GetTypes();

        var indexer = _configuration.TryFindTypeIndexer( target.Type, parameterTypes );
        if ( indexer is null )
            throw new ParsedExpressionUnresolvableIndexerException( target.Type, parameterTypes );

        return target.NodeType == ExpressionType.Constant && callParameters.All( p => p.NodeType == ExpressionType.Constant )
            ? ExpressionHelpers.CreateConstantIndexer( (ConstantExpression)target, indexer, callParameters )
            : CreateVariableIndexer( target, indexer, callParameters );
    }

    [Pure]
    private static Expression CreateVariableIndexer(Expression target, MemberInfo indexer, Expression[] parameters)
    {
        if ( indexer is PropertyInfo property )
            return Expression.MakeIndex( target, property, parameters );

        var arrayMethod = (MethodInfo)indexer;
        return Expression.Call( target, arrayMethod, parameters );
    }
}
