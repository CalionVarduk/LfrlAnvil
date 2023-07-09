using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlNullNode : SqlExpressionNode
{
    internal SqlNullNode()
        : base( SqlNodeType.Null ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "NULL" );
    }
}
