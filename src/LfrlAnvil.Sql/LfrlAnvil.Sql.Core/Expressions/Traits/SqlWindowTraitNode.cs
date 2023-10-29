namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlWindowTraitNode : SqlTraitNode
{
    internal SqlWindowTraitNode(SqlWindowDefinitionNode definition)
        : base( SqlNodeType.WindowTrait )
    {
        Definition = definition;
    }

    public SqlWindowDefinitionNode Definition { get; }
}
