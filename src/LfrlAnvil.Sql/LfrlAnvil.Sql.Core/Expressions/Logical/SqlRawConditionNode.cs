using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a raw SQL condition.
/// </summary>
public sealed class SqlRawConditionNode : SqlConditionNode
{
    internal SqlRawConditionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawCondition )
    {
        Sql = sql;
        Parameters = parameters;
    }

    /// <summary>
    /// Raw SQL condition.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Collection of parameter nodes.
    /// </summary>
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }
}
