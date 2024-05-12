namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node.
/// </summary>
public interface ISqlStatementNode
{
    /// <summary>
    /// Underlying SQL node.
    /// </summary>
    public SqlNodeBase Node { get; }

    /// <summary>
    /// Number of queries contained by this statement node.
    /// </summary>
    public int QueryCount { get; }
}
