using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawExpressionNode : SqlExpressionNode
{
    internal SqlRawExpressionNode(string sql, TypeNullability? type, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawExpression )
    {
        Sql = sql;
        Type = type;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }
    public TypeNullability? Type { get; }
}
