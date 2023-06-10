using System;
using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlRawConditionNode : SqlConditionNode
{
    internal SqlRawConditionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawCondition )
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyMemory<SqlParameterNode> Parameters { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( indent == 0 ? Sql : Sql.Replace( Environment.NewLine, Environment.NewLine + new string( ' ', indent ) ) );
    }
}
