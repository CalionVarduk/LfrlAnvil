using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LfrlAnvil.Expressions;

public class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly IReadOnlyDictionary<string, Expression> _parametersToReplace;

    public ExpressionParameterReplacer(IReadOnlyDictionary<string, Expression> parametersToReplace)
    {
        _parametersToReplace = parametersToReplace;
    }

    [return: NotNullIfNotNull( "node" )]
    public override Expression? Visit(Expression? node)
    {
        if ( node is not ParameterExpression parameterExpression || parameterExpression.Name is null )
            return base.Visit( node );

        return _parametersToReplace.TryGetValue( parameterExpression.Name, out var expression )
            ? expression
            : base.Visit( node );
    }
}
