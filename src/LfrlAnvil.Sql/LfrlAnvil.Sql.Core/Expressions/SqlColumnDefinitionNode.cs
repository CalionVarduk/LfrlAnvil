using System.Text;

namespace LfrlAnvil.Sql.Expressions;

public class SqlColumnDefinitionNode : SqlNodeBase
{
    protected internal SqlColumnDefinitionNode(string name, SqlExpressionType type)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public SqlExpressionType Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '[' ).Append( Name ).Append( ']' );
        AppendTypeTo( builder, Type );
    }
}
