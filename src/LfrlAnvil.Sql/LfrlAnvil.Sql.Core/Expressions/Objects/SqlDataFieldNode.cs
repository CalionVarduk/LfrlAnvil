using System.Text;

namespace LfrlAnvil.Sql.Expressions.Objects;

public abstract class SqlDataFieldNode : SqlExpressionNode
{
    protected SqlDataFieldNode(SqlRecordSetNode recordSet, SqlNodeType nodeType)
        : base( nodeType )
    {
        RecordSet = recordSet;
    }

    public SqlRecordSetNode RecordSet { get; }
    public abstract string Name { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( '[' ).Append( RecordSet.Name ).Append( ']' ).Append( '.' ).Append( '[' ).Append( Name ).Append( ']' );
    }
}
