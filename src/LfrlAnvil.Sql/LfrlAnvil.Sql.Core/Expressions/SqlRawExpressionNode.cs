using System;
using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawExpressionNode : SqlExpressionNode
{
    internal SqlRawExpressionNode(string sql, SqlExpressionType? type, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawExpression )
    {
        Sql = sql;
        Type = type;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyMemory<SqlParameterNode> Parameters { get; }
    public SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( indent == 0 ? Sql : Sql.Replace( Environment.NewLine, Environment.NewLine + new string( ' ', indent ) ) );
    }
}
