using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlInsertIntoNode : SqlNodeBase
{
    internal SqlInsertIntoNode(SqlQueryExpressionNode query, SqlRecordSetNode recordSet, SqlDataFieldNode[] dataFields)
        : base( SqlNodeType.InsertInto )
    {
        Source = query;
        RecordSet = recordSet;
        DataFields = dataFields;
    }

    internal SqlInsertIntoNode(SqlValuesNode values, SqlRecordSetNode recordSet, SqlDataFieldNode[] dataFields)
        : base( SqlNodeType.InsertInto )
    {
        Source = values;
        RecordSet = recordSet;
        DataFields = dataFields;
    }

    public SqlNodeBase Source { get; }
    public SqlRecordSetNode RecordSet { get; }
    public ReadOnlyMemory<SqlDataFieldNode> DataFields { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder, Source, indent );
        AppendTo( builder.Indent( indent ).Append( "INSERT INTO" ).Append( ' ' ), RecordSet, indent );

        if ( DataFields.Length == 0 )
            return;

        var dataFieldIndent = indent + DefaultIndent;
        builder.Indent( indent ).Append( '(' );

        foreach ( var dataField in DataFields.Span )
        {
            AppendChildTo( builder.Indent( dataFieldIndent ), dataField, dataFieldIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
        builder.Indent( indent ).Append( ')' );
    }
}
