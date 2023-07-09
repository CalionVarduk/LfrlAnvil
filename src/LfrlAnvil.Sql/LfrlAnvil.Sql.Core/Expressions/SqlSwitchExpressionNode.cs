using System;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSwitchExpressionNode : SqlExpressionNode
{
    internal SqlSwitchExpressionNode(SqlSwitchCaseNode[] cases, SqlExpressionNode @default)
        : base( SqlNodeType.Switch )
    {
        Ensure.IsNotEmpty( cases, nameof( cases ) );
        Cases = cases;
        Default = @default;
    }

    public ReadOnlyMemory<SqlSwitchCaseNode> Cases { get; }
    public SqlExpressionNode Default { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var caseIndent = indent + DefaultIndent;
        builder.Append( "CASE" );

        foreach ( var @case in Cases.Span )
            AppendTo( builder.Indent( caseIndent ), @case, caseIndent );

        AppendChildTo( builder.Indent( caseIndent ).Append( "ELSE" ).Append( ' ' ), Default, caseIndent );
        builder.Indent( indent ).Append( "END" );
    }
}
