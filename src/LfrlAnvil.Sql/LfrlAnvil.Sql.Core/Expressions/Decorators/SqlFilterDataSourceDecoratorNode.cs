using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlFilterDataSourceDecoratorNode : SqlDataSourceDecoratorNode
{
    internal SqlFilterDataSourceDecoratorNode(SqlConditionNode filter, bool isConjunction)
        : base( SqlNodeType.FilterDecorator )
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
