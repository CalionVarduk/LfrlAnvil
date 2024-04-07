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

    public ParsedExpressionIndexerCall(
        ParsedExpressionFactoryInternalConfiguration configuration,
        bool foldConstantsWhenPossible = true)
    {
        _configuration = configuration;
        FoldConstantsWhenPossible = foldConstantsWhenPossible;
    }

    public bool FoldConstantsWhenPossible { get; }

    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsAtLeast( parameters, 2 );

        var target = parameters[0];
        var callParameters = parameters.Slice( 1 );
        var parameterTypes = callParameters.GetExpressionTypes();

        var indexer = _configuration.TryFindTypeIndexer( target.Type, parameterTypes );
        if ( indexer is null )
            throw new ParsedExpressionUnresolvableIndexerException( target.Type, parameterTypes );

        return FoldConstantsWhenPossible
            && target is ConstantExpression constantTarget
            && callParameters.All( static p => p is ConstantExpression )
                ? ExpressionHelpers.CreateConstantIndexer( constantTarget, indexer, callParameters )
                : CreateVariableIndexer( target, indexer, callParameters );
    }

    [Pure]
    private static Expression CreateVariableIndexer(Expression target, MemberInfo indexer, Expression[] parameters)
    {
        if ( indexer is PropertyInfo property )
            return Expression.MakeIndex( target, property, parameters );

        var arrayMethod = ReinterpretCast.To<MethodInfo>( indexer );
        return Expression.Call( target, arrayMethod, parameters );
    }
}
