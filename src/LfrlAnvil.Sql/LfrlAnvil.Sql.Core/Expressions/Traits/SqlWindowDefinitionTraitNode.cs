namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single window definition trait.
/// </summary>
public sealed class SqlWindowDefinitionTraitNode : SqlTraitNode
{
    internal SqlWindowDefinitionTraitNode(SqlWindowDefinitionNode[] windows)
        : base( SqlNodeType.WindowDefinitionTrait )
    {
        Windows = windows;
    }

    /// <summary>
    /// Collection of window definitions.
    /// </summary>
    public ReadOnlyArray<SqlWindowDefinitionNode> Windows { get; }
}
