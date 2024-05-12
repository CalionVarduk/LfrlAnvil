namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree select node that defines a single expression selection
/// for an <see cref="SqlCompoundQueryExpressionNode"/>.
/// </summary>
public sealed class SqlSelectCompoundFieldNode : SqlSelectNode
{
    internal SqlSelectCompoundFieldNode(string name, Origin[] origins)
        : base( SqlNodeType.SelectCompoundField )
    {
        Name = name;
        Origins = origins;
    }

    /// <summary>
    /// Name of this selected field.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Collection of selections from queries that compose an <see cref="SqlCompoundQueryExpressionNode"/> instance,
    /// which is the source of this field.
    /// </summary>
    public ReadOnlyArray<Origin> Origins { get; }

    internal override void VisitExpressions(ISqlSelectNodeExpressionVisitor visitor)
    {
        visitor.Handle( Name, null );
    }

    /// <summary>
    /// Represents a selection from a single query that is a component of an <see cref="SqlCompoundQueryExpressionNode"/>.
    /// </summary>
    /// <param name="QueryIndex">0-based index of the source query within the compound query expression.</param>
    /// <param name="Selection">Selection from the source query.</param>
    /// <param name="Expression">Expression associated with a data field from the <see cref="Selection"/> from the source query.</param>
    public readonly record struct Origin(int QueryIndex, SqlSelectNode Selection, SqlExpressionNode? Expression);
}
