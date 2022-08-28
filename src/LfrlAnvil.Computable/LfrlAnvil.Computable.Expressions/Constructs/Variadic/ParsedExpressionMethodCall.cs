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

    public ParsedExpressionMethodCall(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        _configuration = configuration;
    }

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

        return target.NodeType == ExpressionType.Constant && callParameters.All( p => p.NodeType == ExpressionType.Constant )
            ? ExpressionHelpers.CreateConstantMethodCall( (ConstantExpression)target, method, callParameters )
            : Expression.Call( target, method, callParameters );
    }
}
