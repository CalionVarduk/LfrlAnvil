using System.Text;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSelectExpressionNode : SqlExpressionNode
{
    internal SqlSelectExpressionNode(SqlSelectNode selection)
        : base( SqlNodeType.SelectExpression )
    {
        Selection = selection;
    }

    public SqlSelectNode Selection { get; }
    public override SqlExpressionType? Type => Selection.Type;

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder, Selection, indent );
    }
}
