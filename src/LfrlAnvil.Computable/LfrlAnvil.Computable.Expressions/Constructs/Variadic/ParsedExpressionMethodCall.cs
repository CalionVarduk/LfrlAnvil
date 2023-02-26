using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionMethodCall : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    public ParsedExpressionMethodCall(
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
        Ensure.ContainsAtLeast( parameters, 2, nameof( parameters ) );

        var target = parameters[0];
        var methodName = parameters[1].GetConstantMemberNameValue();
        var callParameters = parameters.Slice( 2 );
        var parameterTypes = callParameters.GetExpressionTypes();

        var methods = _configuration.FindTypeMethods( target.Type, methodName, parameterTypes );

        if ( methods.Length == 0 )
            throw new ParsedExpressionUnresolvableMemberException( target.Type, methodName, parameterTypes );

        if ( methods.Length > 1 )
            throw new ParsedExpressionMemberAmbiguityException( target.Type, methodName, methods );

        var method = methods[0];

        return FoldConstantsWhenPossible &&
            target is ConstantExpression constantTarget &&
            callParameters.All( static p => p is ConstantExpression )
                ? ExpressionHelpers.CreateConstantMethodCall( constantTarget, method, callParameters )
                : Expression.Call( target, method, callParameters );
    }
}
