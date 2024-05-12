namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a switch expression.
/// </summary>
public sealed class SqlSwitchExpressionNode : SqlExpressionNode
{
    internal SqlSwitchExpressionNode(SqlSwitchCaseNode[] cases, SqlExpressionNode @default)
        : base( SqlNodeType.Switch )
    {
        Ensure.IsNotEmpty( cases );
        Cases = cases;
        Default = @default;
    }

    /// <summary>
    /// Collection of cases.
    /// </summary>
    public ReadOnlyArray<SqlSwitchCaseNode> Cases { get; }

    /// <summary>
    /// Default expression.
    /// </summary>
    public SqlExpressionNode Default { get; }
}
