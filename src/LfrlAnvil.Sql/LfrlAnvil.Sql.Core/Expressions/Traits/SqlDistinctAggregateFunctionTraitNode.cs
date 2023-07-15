using System.Text;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlDistinctAggregateFunctionTraitNode : SqlAggregateFunctionTraitNode
{
    internal SqlDistinctAggregateFunctionTraitNode()
        : base( SqlNodeType.DistinctTrait ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "DISTINCT" );
    }
}
