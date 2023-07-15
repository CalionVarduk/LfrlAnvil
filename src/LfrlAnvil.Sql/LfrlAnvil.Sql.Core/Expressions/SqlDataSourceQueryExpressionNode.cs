using System;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlDataSourceQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    internal SqlDataSourceQueryExpressionNode(Chain<SqlQueryTraitNode> traits)
        : base( SqlNodeType.DataSourceQuery, traits ) { }

    public abstract SqlDataSourceNode DataSource { get; }

    [Pure]
    public abstract SqlDataSourceQueryExpressionNode Select(params SqlSelectNode[] selection);

    [Pure]
    public abstract SqlDataSourceQueryExpressionNode AddTrait(SqlDataSourceTraitNode trait);
}

public sealed class SqlDataSourceQueryExpressionNode<TDataSourceNode> : SqlDataSourceQueryExpressionNode
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlDataSourceQueryExpressionNode(TDataSourceNode dataSource, ReadOnlyMemory<SqlSelectNode> selection)
        : base( Chain<SqlQueryTraitNode>.Empty )
    {
        DataSource = dataSource;
        Selection = selection;
    }

    internal SqlDataSourceQueryExpressionNode(TDataSourceNode dataSource, SqlQueryTraitNode trait)
        : base( Chain.Create( trait ) )
    {
        DataSource = dataSource;
        Selection = ReadOnlyMemory<SqlSelectNode>.Empty;
    }

    private SqlDataSourceQueryExpressionNode(SqlDataSourceQueryExpressionNode<TDataSourceNode> @base, Chain<SqlQueryTraitNode> traits)
        : base( traits )
    {
        DataSource = @base.DataSource;
        Selection = @base.Selection;
    }

    private SqlDataSourceQueryExpressionNode(SqlDataSourceQueryExpressionNode<TDataSourceNode> @base, TDataSourceNode dataSource)
        : base( @base.Traits )
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
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> AddTrait(SqlQueryTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, traits );
    }

    [Pure]
    public override SqlDataSourceQueryExpressionNode<TDataSourceNode> AddTrait(SqlDataSourceTraitNode trait)
    {
        var dataSource = (TDataSourceNode)DataSource.AddTrait( trait );
        return new SqlDataSourceQueryExpressionNode<TDataSourceNode>( this, dataSource );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var selectIndent = indent + DefaultIndent;
        AppendTo( builder, DataSource, indent );

        foreach ( var trait in Traits )
            AppendTo( builder.Indent( indent ), trait, indent );

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
