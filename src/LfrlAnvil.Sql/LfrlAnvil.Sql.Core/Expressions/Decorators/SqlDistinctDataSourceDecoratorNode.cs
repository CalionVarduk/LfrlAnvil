using System.Text;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlDistinctDataSourceDecoratorNode : SqlDataSourceDecoratorNode
{
    internal SqlDistinctDataSourceDecoratorNode()
        : base( SqlNodeType.DistinctDecorator ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "DISTINCT" );
    }
}
