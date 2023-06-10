using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlFalseNode : SqlConditionNode
{
    internal SqlFalseNode()
        : base( SqlNodeType.False ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "FALSE" );
    }
}
