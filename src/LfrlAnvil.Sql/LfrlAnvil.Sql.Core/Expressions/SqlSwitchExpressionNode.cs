using System;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSwitchExpressionNode : SqlExpressionNode
{
    internal SqlSwitchExpressionNode(SqlSwitchCaseNode[] cases, SqlExpressionNode @default)
        : base( SqlNodeType.Switch )
    {
        Ensure.IsNotEmpty( cases, nameof( cases ) );
        Type = GetCommonType( cases, @default.Type );
        Cases = cases;
        Default = @default;
    }

    public ReadOnlyMemory<SqlSwitchCaseNode> Cases { get; }
    public SqlExpressionNode Default { get; }
    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var caseIndent = indent + DefaultIndent;
        builder.Append( "CASE" );

        foreach ( var @case in Cases.Span )
            AppendTo( builder.Indent( caseIndent ), @case, caseIndent );

        AppendChildTo( builder.Indent( caseIndent ).Append( "ELSE" ).Append( ' ' ), Default, caseIndent );
        builder.Indent( indent ).Append( "END" );
    }

    [Pure]
    private static SqlExpressionType? GetCommonType(SqlSwitchCaseNode[] cases, SqlExpressionType? defaultType)
    {
        if ( defaultType is null )
            return null;

        foreach ( var @case in cases )
        {
            if ( @case.Expression.Type is null )
                return null;
        }

        var result = defaultType;
        foreach ( var @case in cases )
            result = SqlExpressionType.GetCommonType( result, @case.Expression.Type );

        return result;
    }
}
