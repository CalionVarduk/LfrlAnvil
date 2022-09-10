using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionConstructorCall : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    public ParsedExpressionConstructorCall(
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
        Ensure.ContainsAtLeast( parameters, 1, nameof( parameters ) );

        var type = parameters[0].GetConstantCtorTypeValue();
        var callParameters = parameters.Slice( 1 );
        var parameterTypes = callParameters.GetExpressionTypes();

        var ctor = _configuration.TryFindTypeCtor( type, parameterTypes );
        if ( ctor is null )
            throw new ParsedExpressionUnresolvableMemberException( type, ".ctor", parameterTypes );

        return FoldConstantsWhenPossible && callParameters.All( p => p is ConstantExpression )
            ? ExpressionHelpers.CreateConstantCtorCall( ctor, callParameters )
            : Expression.New( ctor, callParameters );
    }
}
