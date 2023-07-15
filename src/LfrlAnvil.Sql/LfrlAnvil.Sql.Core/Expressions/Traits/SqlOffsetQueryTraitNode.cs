using System.Text;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlOffsetQueryTraitNode : SqlQueryTraitNode
{
    internal SqlOffsetQueryTraitNode(SqlExpressionNode value)
        : base( SqlNodeType.OffsetTrait )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "OFFSET" ).Append( ' ' ), Value, indent );
    }
}
