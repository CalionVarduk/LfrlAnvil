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
    public ReadOnlyArray<SqlExpressionNode> Partitioning { get; }
    public ReadOnlyArray<SqlOrderByNode> Ordering { get; }
    public SqlWindowFrameNode? Frame { get; }
}
