using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlAggregationFilterDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlAggregationFilterDataSourceDecoratorNode(TDataSourceNode dataSource, SqlConditionNode filter)
        : base( SqlNodeType.AggregationFilterDecorator, dataSource )
    {
        Filter = filter;
        IsConjunction = true;
    }

    internal SqlAggregationFilterDataSourceDecoratorNode(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlConditionNode filter,
        bool isConjunction)
        : base( SqlNodeType.AggregationFilterDecorator, @base )
    {
        Filter = filter;
        IsConjunction = isConjunction;
    }

    public SqlConditionNode Filter { get; }
    public bool IsConjunction { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var filterIndent = indent + DefaultIndent;
        builder.Append( IsConjunction ? "AND" : "OR" ).Append( ' ' ).Append( "HAVING" ).Indent( filterIndent );
        AppendChildTo( builder, Filter, filterIndent );
    }
}
