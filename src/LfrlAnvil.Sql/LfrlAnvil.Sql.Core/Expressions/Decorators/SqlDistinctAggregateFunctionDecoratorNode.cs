using System.Text;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlDistinctAggregateFunctionDecoratorNode : SqlAggregateFunctionDecoratorNode
{
    internal SqlDistinctAggregateFunctionDecoratorNode()
        : base( SqlNodeType.DistinctDecorator ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "DISTINCT" );
    }
}
