using System;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlWindowDefinitionTraitNode : SqlTraitNode
{
    internal SqlWindowDefinitionTraitNode(SqlWindowDefinitionNode[] windows)
        : base( SqlNodeType.WindowDefinitionTrait )
    {
        Windows = windows;
    }

    public ReadOnlyMemory<SqlWindowDefinitionNode> Windows { get; }
}
