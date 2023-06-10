using System;
using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlInConditionNode : SqlConditionNode
{
    internal SqlInConditionNode(SqlExpressionNode value, SqlExpressionNode[] expressions, bool isNegated)
        : base( SqlNodeType.In )
    {
        Assume.IsNotEmpty( expressions, nameof( expressions ) );
        Value = value;
        Expressions = expressions;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public ReadOnlyMemory<SqlExpressionNode> Expressions { get; }
    public bool IsNegated { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder, Value, indent );

        builder.Append( ' ' );
        if ( IsNegated )
            builder.Append( "NOT" ).Append( ' ' );

        builder.Append( "IN" ).Append( ' ' ).Append( '(' );
        foreach ( var expression in Expressions.Span )
        {
            AppendChildTo( builder, expression, indent );
            builder.Append( ',' ).Append( ' ' );
        }

        builder.Length -= 2;
        builder.Append( ')' );
    }
}
