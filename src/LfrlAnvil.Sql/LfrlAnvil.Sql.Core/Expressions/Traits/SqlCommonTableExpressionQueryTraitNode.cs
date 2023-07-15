using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlCommonTableExpressionQueryTraitNode : SqlQueryTraitNode
{
    internal SqlCommonTableExpressionQueryTraitNode(SqlCommonTableExpressionNode[] commonTableExpressions)
        : base( SqlNodeType.CommonTableExpressionTrait )
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
