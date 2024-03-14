namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlWindowDefinitionTraitNode : SqlTraitNode
{
    internal SqlWindowDefinitionTraitNode(SqlWindowDefinitionNode[] windows)
        : base( SqlNodeType.WindowDefinitionTrait )
    {
        Windows = windows;
    }

    public ReadOnlyArray<SqlWindowDefinitionNode> Windows { get; }
}
