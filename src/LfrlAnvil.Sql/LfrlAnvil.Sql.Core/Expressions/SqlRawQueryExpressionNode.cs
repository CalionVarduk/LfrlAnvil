using System;
using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawQueryExpressionNode : SqlExpressionNode
{
    internal SqlRawQueryExpressionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawQuery )
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyMemory<SqlParameterNode> Parameters { get; }
    public override SqlExpressionType? Type => null;

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( indent == 0 ? Sql : Sql.Replace( Environment.NewLine, Environment.NewLine + new string( ' ', indent ) ) );
    }
}
