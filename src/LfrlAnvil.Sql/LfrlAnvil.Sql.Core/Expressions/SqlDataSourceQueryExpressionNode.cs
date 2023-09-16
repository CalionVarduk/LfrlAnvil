using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlDataSourceQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    internal SqlDataSourceQueryExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlNodeType.DataSourceQuery, traits ) { }

    public abstract SqlDataSourceNode DataSource { get; }

    [Pure]
    public abstract SqlDataSourceQueryExpressionNode Select(params SqlSelectNode[] selection);
}

public sealed class SqlDataSourceQueryExpressionNode<TDataSourceNode> : SqlDataSourceQueryExpressionNode
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlDataSourceQueryExpressionNode(TDataSourceNode dataSource, ReadOnlyMemory<SqlSelectNode> selection)
        : base( Chain<SqlTraitNode>.Empty )
    {
        DataSource = dataSource;
        Selection = selection;
    }

    private SqlDataSourceQueryExpressionNode(SqlDataSourceQueryExpressionNode<TDataSourceNode> @base, Chain<SqlTraitNode> traits)
        : base( traits )
    {
        DataSource = @base.DataSource;
        Selection = @base.Selection;
    }

    private SqlDataSourceQueryExpressionNode(
        SqlDataSourceQueryExpressionNode<TDataSourceNode> @base,
        ReadOnlyMemory<SqlSelectNode> selection)
        : base( @base.Traits )
    {
        DataSource = @base.DataSource;
        Selection = selection;
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
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, newSelection );
    }

    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, traits );
    }
}
