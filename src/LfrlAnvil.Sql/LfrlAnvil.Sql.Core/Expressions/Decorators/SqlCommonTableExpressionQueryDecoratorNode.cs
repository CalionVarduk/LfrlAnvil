using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlCommonTableExpressionQueryDecoratorNode : SqlQueryDecoratorNode
{
    internal SqlCommonTableExpressionQueryDecoratorNode(SqlCommonTableExpressionNode[] commonTableExpressions)
        : base( SqlNodeType.CommonTableExpressionDecorator )
    {
        CommonTableExpressions = commonTableExpressions;
    }

    public ReadOnlyMemory<SqlCommonTableExpressionNode> CommonTableExpressions { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var cteIndent = indent + DefaultIndent;
        builder.Append( "WITH" );

        if ( CommonTableExpressions.Length == 0 )
            return;

        foreach ( var cte in CommonTableExpressions.Span )
        {
            AppendTo( builder.Indent( cteIndent ), cte, cteIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
    }
}
