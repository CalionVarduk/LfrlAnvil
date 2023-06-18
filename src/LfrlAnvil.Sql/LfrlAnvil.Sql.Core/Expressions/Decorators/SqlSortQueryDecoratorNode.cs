using System;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlSortQueryDecoratorNode : SqlQueryDecoratorNode
{
    internal SqlSortQueryDecoratorNode(SqlOrderByNode[] ordering)
        : base( SqlNodeType.SortDecorator )
    {
        Ordering = ordering;
    }

    public ReadOnlyMemory<SqlOrderByNode> Ordering { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var orderIndent = indent + DefaultIndent;
        builder.Append( "ORDER BY" );

        if ( Ordering.Length == 0 )
            return;

        foreach ( var orderBy in Ordering.Span )
        {
            AppendTo( builder.Indent( orderIndent ), orderBy, orderIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
    }
}
