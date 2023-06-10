using System;
using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlNullNode : SqlExpressionNode
{
    internal SqlNullNode()
        : base( SqlNodeType.Null )
    {
        Type = SqlExpressionType.Create<DBNull>();
    }

    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "NULL" );
    }
}
