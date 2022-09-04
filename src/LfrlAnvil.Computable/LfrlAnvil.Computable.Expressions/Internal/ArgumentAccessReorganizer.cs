using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Internal;

internal sealed class ArgumentAccessReorganizer : ExpressionVisitor
{
    private readonly ParameterExpression _parameter;
    private readonly IReadOnlyDictionary<int, Expression> _argumentAccessExpressions;
    private readonly int _oldArgumentCount;

    internal ArgumentAccessReorganizer(
        ParameterExpression parameter,
        IReadOnlyDictionary<int, Expression> argumentAccessExpressions,
        int oldArgumentCount)
    {
        _parameter = parameter;
        _argumentAccessExpressions = argumentAccessExpressions;
        _oldArgumentCount = oldArgumentCount;
    }

    [Pure]
    [return: NotNullIfNotNull( "node" )]
    public override Expression? Visit(Expression? node)
    {
        if ( ! node.TryGetArgumentAccessIndex( _parameter, _oldArgumentCount, out var oldIndex ) )
            return base.Visit( node );

        var result = _argumentAccessExpressions[oldIndex];
        return result;
    }
}
