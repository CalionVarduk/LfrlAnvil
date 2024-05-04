using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using LfrlAnvil.Computable.Expressions.Exceptions;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.Variadic;

/// <summary>
/// Represents a member access construct.
/// </summary>
public sealed class ParsedExpressionMemberAccess : ParsedExpressionVariadicFunction
{
    private readonly ParsedExpressionFactoryInternalConfiguration _configuration;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionMemberAccess"/> instance.
    /// </summary>
    /// <param name="configuration">Underlying configuration.</param>
    /// <param name="foldConstantsWhenPossible">
    /// Specifies whether or not member access for constant target
    /// should be resolved immediately as constant expression. Equal to <b>true</b> by default.
    /// </param>
    public ParsedExpressionMemberAccess(
        ParsedExpressionFactoryInternalConfiguration configuration,
        bool foldConstantsWhenPossible = true)
    {
        _configuration = configuration;
        FoldConstantsWhenPossible = foldConstantsWhenPossible;
    }

    /// <summary>
    /// Specifies whether or not member access for constant target
    /// should be resolved immediately as constant expression.
    /// </summary>
    public bool FoldConstantsWhenPossible { get; }

    /// <inheritdoc />
    [Pure]
    protected internal override Expression Process(IReadOnlyList<Expression> parameters)
    {
        Ensure.ContainsExactly( parameters, 2 );

        var target = parameters[0];
        var memberName = parameters[1].GetConstantMemberNameValue();

        var members = _configuration.FindTypeFieldsAndProperties( target.Type, memberName );

        if ( members.Length == 0 )
            throw new ParsedExpressionUnresolvableMemberException( target.Type, memberName );

        if ( members.Length > 1 )
            throw new ParsedExpressionMemberAmbiguityException( target.Type, memberName, members );

        var member = members[0];

        return FoldConstantsWhenPossible && target is ConstantExpression constantTarget
            ? ExpressionHelpers.CreateConstantMemberAccess( constantTarget, member )
            : Expression.MakeMemberAccess( target, member );
    }
}
