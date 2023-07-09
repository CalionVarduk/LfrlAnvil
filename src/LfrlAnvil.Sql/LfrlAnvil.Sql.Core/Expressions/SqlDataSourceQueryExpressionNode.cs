using System;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Decorators;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlDataSourceQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    internal SqlDataSourceQueryExpressionNode(Chain<SqlQueryDecoratorNode> decorators)
        : base( SqlNodeType.DataSourceQuery, decorators ) { }

    public abstract SqlDataSourceNode DataSource { get; }

    [Pure]
    public abstract SqlDataSourceQueryExpressionNode Select(params SqlSelectNode[] selection);

    [Pure]
    public abstract SqlDataSourceQueryExpressionNode Decorate(SqlDataSourceDecoratorNode decorator);
}

public sealed class SqlDataSourceQueryExpressionNode<TDataSourceNode> : SqlDataSourceQueryExpressionNode
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlDataSourceQueryExpressionNode(TDataSourceNode dataSource, ReadOnlyMemory<SqlSelectNode> selection)
        : base( Chain<SqlQueryDecoratorNode>.Empty )
    {
        DataSource = dataSource;
        Selection = selection;
    }

    internal SqlDataSourceQueryExpressionNode(TDataSourceNode dataSource, SqlQueryDecoratorNode decorator)
        : base( Chain.Create( decorator ) )
    {
        DataSource = dataSource;
        Selection = ReadOnlyMemory<SqlSelectNode>.Empty;
    }

    private SqlDataSourceQueryExpressionNode(
        SqlDataSourceQueryExpressionNode<TDataSourceNode> @base,
        Chain<SqlQueryDecoratorNode> decorators)
        : base( decorators )
    {
        DataSource = @base.DataSource;
        Selection = @base.Selection;
    }

    private SqlDataSourceQueryExpressionNode(SqlDataSourceQueryExpressionNode<TDataSourceNode> @base, TDataSourceNode dataSource)
        : base( @base.Decorators )
    {
        DataSource = dataSource;
        Selection = @base.Selection;
    }

    public override TDataSourceNode DataSource { get; }
    public override ReadOnlyMemory<SqlSelectNode> Selection { get; }

    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> Select(params SqlSelectNode[] selection)
    {
        if ( selection.Length == 0 )
            return this;

        var newSelection = new SqlSelectNode[Selection.Length + selection.Length];
        Selection.CopyTo( newSelection );
        selection.CopyTo( newSelection, Selection.Length );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( DataSource, newSelection );
    }

    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> Decorate(SqlQueryDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, decorators );
    }

    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> Decorate(SqlDataSourceDecoratorNode decorator)
    {
        var dataSource = (TDataSourceNode)DataSource.Decorate( decorator );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, dataSource );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var selectIndent = indent + DefaultIndent;
        AppendTo( builder, DataSource, indent );

        foreach ( var decorator in Decorators )
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
