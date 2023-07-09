using System;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlTypeCastExpressionNode : SqlExpressionNode
{
    internal SqlTypeCastExpressionNode(SqlExpressionNode node, Type targetType)
        : base( SqlNodeType.TypeCast )
    {
        Node = node;
        TargetType = targetType;
    }

    public SqlExpressionNode Node { get; }
    public Type TargetType { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "CAST" ).Append( '(' ), Node, indent );
        builder.Append( ' ' ).Append( "AS" ).Append( ' ' ).Append( TargetType.GetDebugString() ).Append( ')' );
    }
}
