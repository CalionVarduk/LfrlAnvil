using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

public sealed class ParsedExpressionMemberAccess : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    public ParsedExpressionMemberAccess(ParsedExpressionFactoryInternalConfiguration configuration)
    {
        _configuration = configuration;
    }

    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsExactly( parameters, 2, nameof( parameters ) );

        var target = parameters[0];
        var memberName = parameters[1].GetConstantMemberNameValue();

        var members = _configuration.FindTypeFieldsAndProperties( target.Type, memberName );

        if ( members.Length == 0 )
            throw new ParsedExpressionUnresolvableMemberException( target.Type, memberName );

        if ( members.Length > 1 )
            throw new ParsedExpressionMemberAmbiguityException( target.Type, memberName, members );

        var member = members[0];

        return target.NodeType == ExpressionType.Constant
            ? ExpressionHelpers.CreateConstantMemberAccess( (ConstantExpression)target, member )
            : Expression.MakeMemberAccess( target, member );
    }
}
