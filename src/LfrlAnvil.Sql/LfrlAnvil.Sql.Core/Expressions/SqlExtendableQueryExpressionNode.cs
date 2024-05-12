using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a query expression that can be decorated with traits.
/// </summary>
public abstract class SqlExtendableQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlExtendableQueryExpressionNode(SqlNodeType nodeType, Chain<SqlTraitNode> traits)
        : base( nodeType )
    {
        Traits = traits;
    }

    /// <summary>
    /// Collection of decorating traits.
    /// </summary>
    public Chain<SqlTraitNode> Traits { get; }

    /// <summary>
    /// Creates a new SQL query expression syntax tree node by adding a new <paramref name="trait"/>.
    /// </summary>
    /// <param name="trait">Trait to add.</param>
    /// <returns>New SQL query expression syntax tree node.</returns>
    [Pure]
    public abstract SqlExtendableQueryExpressionNode AddTrait(SqlTraitNode trait);

    /// <summary>
    /// Creates a new SQL query expression syntax tree node by changing the <see cref="Traits"/> collection.
    /// </summary>
    /// <param name="traits">Collection of traits to set.</param>
    /// <returns>New SQL query expression syntax tree node.</returns>
    [Pure]
    public abstract SqlExtendableQueryExpressionNode SetTraits(Chain<SqlTraitNode> traits);
}
