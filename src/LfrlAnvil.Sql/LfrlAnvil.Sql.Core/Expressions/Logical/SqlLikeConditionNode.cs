using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlLikeConditionNode : SqlConditionNode
{
    internal SqlLikeConditionNode(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape, bool isNegated)
        : base( SqlNodeType.Like )
    {
        Value = value;
        Pattern = pattern;
        Escape = escape;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public SqlExpressionNode Pattern { get; }
    public SqlExpressionNode? Escape { get; }
    public bool IsNegated { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder, Value, indent );

        builder.Append( ' ' );
        if ( IsNegated )
            builder.Append( "NOT" ).Append( ' ' );

        AppendChildTo( builder.Append( "LIKE" ).Append( ' ' ), Pattern, indent );
        if ( Escape is not null )
            AppendChildTo( builder.Append( ' ' ).Append( "ESCAPE" ).Append( ' ' ), Escape, indent );
    }
}
