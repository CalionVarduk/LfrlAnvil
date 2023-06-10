using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlExistsConditionNode : SqlConditionNode
{
    internal SqlExistsConditionNode(SqlQueryExpressionNode query, bool isNegated)
        : base( SqlNodeType.Exists )
    {
        Query = query;
        IsNegated = isNegated;
    }

    public SqlQueryExpressionNode Query { get; }
    public bool IsNegated { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;

        if ( IsNegated )
            builder.Append( "NOT" ).Append( ' ' );

        builder.Append( "EXISTS" ).Append( ' ' ).Append( '(' ).Indent( queryIndent );
        AppendTo( builder, Query, queryIndent );
        builder.Indent( indent ).Append( ')' );
    }
}
