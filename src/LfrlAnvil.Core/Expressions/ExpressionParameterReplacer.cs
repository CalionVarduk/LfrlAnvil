using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LfrlAnvil.Expressions;

public class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression[] _parametersToReplace;
    private readonly Expression[] _replacements;

    public ExpressionParameterReplacer(ParameterExpression[] parametersToReplace, Expression[] replacements)
    {
        _parametersToReplace = parametersToReplace;
        _replacements = replacements;
    }

    [return: NotNullIfNotNull( "node" )]
    public override Expression? Visit(Expression? node)
    {
        if ( node is null || node.NodeType != ExpressionType.Parameter || node is not ParameterExpression parameterExpression )
            return base.Visit( node );

        var index = Array.IndexOf( _parametersToReplace, parameterExpression );
        return index >= 0 && index < _replacements.Length
            ? _replacements[index]
            : base.Visit( node );
    }
}
