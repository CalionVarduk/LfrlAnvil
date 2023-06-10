using System.Text;

namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseNotExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseNotExpressionNode(SqlExpressionNode value)
        : base( SqlNodeType.BitwiseNot )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }
    public override SqlExpressionType? Type => Value.Type;

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( '~' ), Value, indent );
    }
}
