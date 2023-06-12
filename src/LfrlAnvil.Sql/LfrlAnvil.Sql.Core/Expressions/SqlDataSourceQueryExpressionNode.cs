using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDataSourceQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlDataSourceQueryExpressionNode(SqlDataSourceNode dataSource, SqlSelectNode[] selection)
        : base( SqlNodeType.Query )
    {
        DataSource = dataSource;
        Selection = selection;
        Decorator = null;
        Type = selection.Length != 1 ? null : selection[0].Type?.MakeNullable();
    }

    internal SqlDataSourceQueryExpressionNode(SqlDataSourceDecoratorNode decorator, SqlSelectNode[] selection)
        : base( SqlNodeType.Query )
    {
        DataSource = decorator.DataSource;
        Selection = selection;
        Decorator = decorator;
        Type = selection.Length != 1 ? null : selection[0].Type?.MakeNullable();
    }

    public SqlDataSourceNode DataSource { get; }
    public SqlDataSourceDecoratorNode? Decorator { get; }
    public override ReadOnlyMemory<SqlSelectNode> Selection { get; }
    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var selectIndent = indent + DefaultIndent;

        AppendTo( builder, DataSource, indent );

        var decorators = Decorator?.Reduce() ?? Array.Empty<SqlDataSourceDecoratorNode>();
        foreach ( var decorator in decorators )
            AppendTo( builder.Indent( indent ), decorator, indent );

        builder.Indent( indent ).Append( "SELECT" );

        if ( Selection.Length > 0 )
        {
            foreach ( var field in Selection.Span )
            {
                AppendTo( builder.Indent( selectIndent ), field, selectIndent );
                builder.Append( ',' );
            }

            builder.Length -= 1;
        }
    }
}
