using System;
using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlRawQueryExpressionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawQuery )
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyMemory<SqlParameterNode> Parameters { get; }
    public override ReadOnlyMemory<SqlSelectNode> Selection => ReadOnlyMemory<SqlSelectNode>.Empty;

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( indent == 0 ? Sql : Sql.Replace( Environment.NewLine, Environment.NewLine + new string( ' ', indent ) ) );
    }
}
