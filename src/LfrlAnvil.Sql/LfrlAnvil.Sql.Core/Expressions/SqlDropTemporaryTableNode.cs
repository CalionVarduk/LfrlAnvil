using System.Text;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropTemporaryTableNode : SqlNodeBase
{
    internal SqlDropTemporaryTableNode(string name)
        : base( SqlNodeType.DropTemporaryTable )
    {
        Name = name;
    }

    public string Name { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "DROP TEMPORARY TABLE" ).Append( ' ' ).Append( '[' ).Append( Name ).Append( ']' );
    }
}
