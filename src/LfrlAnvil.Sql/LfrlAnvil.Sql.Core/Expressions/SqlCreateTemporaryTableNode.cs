using System;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateTemporaryTableNode : SqlNodeBase
{
    internal SqlCreateTemporaryTableNode(string name, SqlColumnDefinitionNode[] columns)
        : base( SqlNodeType.CreateTemporaryTable )
    {
        Name = name;
        Columns = columns;
    }

    public string Name { get; }
    public ReadOnlyMemory<SqlColumnDefinitionNode> Columns { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "CREATE TEMPORARY TABLE" ).Append( ' ' ).Append( '[' ).Append( Name ).Append( ']' );

        if ( Columns.Length == 0 )
            return;

        var columnIndent = indent + DefaultIndent;
        builder.Indent( indent ).Append( '(' );

        foreach ( var column in Columns.Span )
        {
            AppendChildTo( builder.Indent( columnIndent ), column, columnIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
        builder.Indent( indent ).Append( ')' );
    }
}
