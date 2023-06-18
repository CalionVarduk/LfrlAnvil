using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlAggregationFilterDataSourceDecoratorNode : SqlDataSourceDecoratorNode
{
    internal SqlAggregationFilterDataSourceDecoratorNode(SqlConditionNode filter, bool isConjunction)
        : base( SqlNodeType.AggregationFilterDecorator )
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
