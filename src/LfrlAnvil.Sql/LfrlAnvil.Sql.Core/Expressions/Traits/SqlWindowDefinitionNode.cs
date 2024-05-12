namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a window.
/// </summary>
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

    /// <summary>
    /// Window's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Collection of expressions by which this window partitions the result set.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Partitioning { get; }

    /// <summary>
    /// Collection of ordering expressions used by this window.
    /// </summary>
    public ReadOnlyArray<SqlOrderByNode> Ordering { get; }

    /// <summary>
    /// Optional <see cref="SqlWindowFrameNode"/> instance that defines the frame of this window.
    /// </summary>
    public SqlWindowFrameNode? Frame { get; }
}
