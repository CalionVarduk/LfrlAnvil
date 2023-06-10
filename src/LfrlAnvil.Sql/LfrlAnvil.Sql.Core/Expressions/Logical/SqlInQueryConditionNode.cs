using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlInQueryConditionNode : SqlConditionNode
{
    internal SqlInQueryConditionNode(SqlExpressionNode value, SqlQueryExpressionNode query, bool isNegated)
        : base( SqlNodeType.InQuery )
    {
        Value = value;
        Query = query;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public SqlQueryExpressionNode Query { get; }
    public bool IsNegated { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;
        AppendChildTo( builder, Value, indent );

        builder.Append( ' ' );
        if ( IsNegated )
            builder.Append( "NOT" ).Append( ' ' );

        builder.Append( "IN" ).Append( ' ' ).Append( '(' ).Indent( queryIndent );
        AppendTo( builder, Query, queryIndent );
        builder.Indent( indent ).Append( ')' );
    }
}
