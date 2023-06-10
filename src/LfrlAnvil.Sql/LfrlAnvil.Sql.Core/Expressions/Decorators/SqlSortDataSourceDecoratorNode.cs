using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlSortDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlSortDataSourceDecoratorNode(TDataSourceNode dataSource, SqlOrderByNode[] ordering)
        : base( SqlNodeType.SortDecorator, dataSource )
    {
        Ordering = ordering;
    }

    internal SqlSortDataSourceDecoratorNode(SqlDataSourceDecoratorNode<TDataSourceNode> @base, SqlOrderByNode[] ordering)
        : base( SqlNodeType.SortDecorator, @base )
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
