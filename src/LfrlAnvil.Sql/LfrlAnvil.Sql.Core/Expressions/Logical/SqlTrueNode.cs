using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlTrueNode : SqlConditionNode
{
    internal SqlTrueNode()
        : base( SqlNodeType.True ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "TRUE" );
    }
}
