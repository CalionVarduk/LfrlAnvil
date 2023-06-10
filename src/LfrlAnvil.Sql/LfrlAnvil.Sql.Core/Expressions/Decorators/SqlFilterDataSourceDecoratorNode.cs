using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlFilterDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlFilterDataSourceDecoratorNode(TDataSourceNode dataSource, SqlConditionNode filter, bool isConjunction)
        : base( SqlNodeType.FilterDecorator, dataSource )
    {
        Filter = filter;
        IsConjunction = isConjunction;
    }

    internal SqlFilterDataSourceDecoratorNode(
        SqlDataSourceDecoratorNode<TDataSourceNode> @base,
        SqlConditionNode filter,
        bool isConjunction)
        : base( SqlNodeType.FilterDecorator, @base )
    {
        Filter = filter;
        IsConjunction = isConjunction;
    }

    public SqlConditionNode Filter { get; }
    public bool IsConjunction { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var filterIndent = indent + DefaultIndent;
        builder.Append( IsConjunction ? "AND" : "OR" ).Append( ' ' ).Append( "WHERE" ).Indent( filterIndent );
        AppendChildTo( builder, Filter, filterIndent );
    }
}
