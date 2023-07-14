using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlUpdateNode : SqlNodeBase
{
    internal SqlUpdateNode(SqlDataSourceNode dataSource, SqlRecordSetNode recordSet, SqlValueAssignmentNode[] assignments)
        : base( SqlNodeType.Update )
    {
        DataSource = dataSource;
        RecordSet = recordSet;
        Assignments = assignments;
    }

    public SqlDataSourceNode DataSource { get; }
    public SqlRecordSetNode RecordSet { get; }
    public ReadOnlyMemory<SqlValueAssignmentNode> Assignments { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder, DataSource, indent );
        AppendTo( builder.Indent( indent ).Append( "UPDATE" ).Append( ' ' ), RecordSet, indent );
        builder.Indent( indent ).Append( "SET" );

        if ( Assignments.Length == 0 )
            return;

        var assignmentIndent = indent + DefaultIndent;
        foreach ( var assignment in Assignments.Span )
        {
            AppendTo( builder.Indent( assignmentIndent ), assignment, assignmentIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
    }
}
