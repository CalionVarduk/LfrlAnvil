using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace LfrlAnvil.Expressions;

/// <summary>
/// Represents an expression tree rewriter capable of replacing <see cref="ParameterExpression"/> nodes by position.
/// </summary>
public class ExpressionParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression[] _parametersToReplace;
    private readonly Expression[] _replacements;

    /// <summary>
    /// Creates a new <see cref="ExpressionParameterReplacer"/> instance.
    /// </summary>
    /// <param name="parametersToReplace">Collection of <see cref="ParameterExpression"/> nodes to replace.</param>
    /// <param name="replacements">Collection of replacement <see cref="Expression"/> nodes.</param>
    /// <remarks>
    /// <see cref="ParameterExpression"/> nodes that do not exist in the <paramref name="parametersToReplace"/> collection will be ignored.
    /// Replacement nodes are chosen by index, that is, if <see cref="ParameterExpression"/> exists
    /// in the <paramref name="parametersToReplace"/> collection, then the index of its first occurrence is used to find its replacement
    /// in the <paramref name="replacements"/> collection.
    /// </remarks>
    public ExpressionParameterReplacer(ParameterExpression[] parametersToReplace, Expression[] replacements)
    {
        _parametersToReplace = parametersToReplace;
        _replacements = replacements;
    }

    /// <inheritdoc />
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
