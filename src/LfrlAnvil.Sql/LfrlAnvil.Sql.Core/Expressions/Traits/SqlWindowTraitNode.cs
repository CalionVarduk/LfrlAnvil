namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single window trait.
/// </summary>
public sealed class SqlWindowTraitNode : SqlTraitNode
{
    internal SqlWindowTraitNode(SqlWindowDefinitionNode definition)
        : base( SqlNodeType.WindowTrait )
    {
        Definition = definition;
    }

    /// <summary>
    /// Underlying window definition.
    /// </summary>
    public SqlWindowDefinitionNode Definition { get; }
}
