using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlDeleteFromNode : SqlNodeBase
{
    internal SqlDeleteFromNode(SqlDataSourceNode dataSource, SqlRecordSetNode recordSet)
        : base( SqlNodeType.DeleteFrom )
    {
        DataSource = dataSource;
        RecordSet = recordSet;
    }

    public SqlDataSourceNode DataSource { get; }
    public SqlRecordSetNode RecordSet { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder, DataSource, indent );
        AppendTo( builder.Indent( indent ).Append( "DELETE" ).Append( ' ' ), RecordSet, indent );
    }
}
