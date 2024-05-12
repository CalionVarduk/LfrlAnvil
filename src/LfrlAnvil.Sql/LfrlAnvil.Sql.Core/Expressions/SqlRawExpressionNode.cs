using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a raw SQL expression.
/// </summary>
public sealed class SqlRawExpressionNode : SqlExpressionNode
{
    internal SqlRawExpressionNode(string sql, TypeNullability? type, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawExpression )
    {
        Sql = sql;
        Type = type;
        Parameters = parameters;
    }

    /// <summary>
    /// Raw SQL expression.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Collection of parameter nodes.
    /// </summary>
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }

    /// <summary>
    /// Optional runtime type of the result of this expression.
    /// </summary>
    public TypeNullability? Type { get; }
}
