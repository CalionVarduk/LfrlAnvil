using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlParameterNode : SqlExpressionNode
{
    internal SqlParameterNode(string name, SqlExpressionType? type)
        : base( SqlNodeType.Parameter )
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTypeTo( builder.Append( '@' ).Append( Name ), Type );
    }
}
