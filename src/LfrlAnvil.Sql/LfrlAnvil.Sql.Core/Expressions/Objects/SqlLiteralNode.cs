using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlLiteralNode<T> : SqlExpressionNode
    where T : notnull
{
    internal SqlLiteralNode(T value)
        : base( SqlNodeType.Literal )
    {
        Value = value;
        Type = SqlExpressionType.Create<T>();
    }

    public T Value { get; }
    public SqlExpressionType Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '"' ).Append( Value ).Append( '"' );
        AppendTypeTo( builder, Type );
    }
}
