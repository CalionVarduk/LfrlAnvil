namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single trait.
/// </summary>
public abstract class SqlTraitNode : SqlNodeBase
{
    /// <summary>
    /// Creates a new <see cref="SqlTraitNode"/> instance with <see cref="SqlNodeType.Unknown"/> type.
    /// </summary>
    protected SqlTraitNode()
        : this( SqlNodeType.Unknown ) { }

    internal SqlTraitNode(SqlNodeType nodeType)
        : base( nodeType ) { }
}
