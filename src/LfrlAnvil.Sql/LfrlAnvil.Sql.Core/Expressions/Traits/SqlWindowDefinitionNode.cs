using System;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlWindowDefinitionNode : SqlNodeBase
{
    internal SqlWindowDefinitionNode(
        string name,
        SqlExpressionNode[] partitioning,
        SqlOrderByNode[] ordering,
        SqlWindowFrameNode? frame)
        : base( SqlNodeType.WindowDefinition )
    {
        Name = name;
        Partitioning = partitioning;
        Ordering = ordering;
        Frame = frame;
    }

    public string Name { get; }
    public ReadOnlyMemory<SqlExpressionNode> Partitioning { get; }
    public ReadOnlyMemory<SqlOrderByNode> Ordering { get; }
    public SqlWindowFrameNode? Frame { get; }
}
