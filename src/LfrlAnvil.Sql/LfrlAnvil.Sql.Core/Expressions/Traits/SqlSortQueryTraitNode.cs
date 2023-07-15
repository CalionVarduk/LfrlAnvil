using System;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlSortQueryTraitNode : SqlQueryTraitNode
{
    internal SqlSortQueryTraitNode(SqlOrderByNode[] ordering)
        : base( SqlNodeType.SortTrait )
    {
        Ordering = ordering;
    }

    public ReadOnlyMemory<SqlOrderByNode> Ordering { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "ORDER BY" );

        if ( Ordering.Length == 0 )
            return;

        var orderIndent = indent + DefaultIndent;
        foreach ( var orderBy in Ordering.Span )
        {
            AppendTo( builder.Indent( orderIndent ), orderBy, orderIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
    }
}
