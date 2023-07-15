using System.Text;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlLimitQueryTraitNode : SqlQueryTraitNode
{
    internal SqlLimitQueryTraitNode(SqlExpressionNode value)
        : base( SqlNodeType.LimitTrait )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "LIMIT" ).Append( ' ' ), Value, indent );
    }
}
