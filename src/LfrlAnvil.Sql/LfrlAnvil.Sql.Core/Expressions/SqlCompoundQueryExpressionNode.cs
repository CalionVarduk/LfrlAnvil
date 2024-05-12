using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a compound query expression.
/// </summary>
public sealed class SqlCompoundQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    private ReadOnlyArray<SqlSelectNode>? _selection;

    internal SqlCompoundQueryExpressionNode(
        SqlQueryExpressionNode firstQuery,
        SqlCompoundQueryComponentNode[] followingQueries)
        : base( SqlNodeType.CompoundQuery, Chain<SqlTraitNode>.Empty )
    {
        Ensure.IsNotEmpty( followingQueries );
        FirstQuery = firstQuery;
        FollowingQueries = followingQueries;
        _selection = null;
    }

    private SqlCompoundQueryExpressionNode(SqlCompoundQueryExpressionNode @base, Chain<SqlTraitNode> traits)
        : base( SqlNodeType.CompoundQuery, traits )
    {
        FirstQuery = @base.FirstQuery;
        FollowingQueries = @base.FollowingQueries;
        _selection = @base._selection;
    }

    /// <summary>
    /// First underlying query.
    /// </summary>
    public SqlQueryExpressionNode FirstQuery { get; }

    /// <summary>
    /// Collection of queries that sequentially follow after the <see cref="FirstQuery"/>.
    /// </summary>
    public ReadOnlyArray<SqlCompoundQueryComponentNode> FollowingQueries { get; }

    /// <inheritdoc />
    public override ReadOnlyArray<SqlSelectNode> Selection => _selection ??= CreateSelection();

    /// <inheritdoc />
    [Pure]
    public override SqlCompoundQueryExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlCompoundQueryExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlCompoundQueryExpressionNode( this, traits );
    }

    [Pure]
    private ReadOnlyArray<SqlSelectNode> CreateSelection()
    {
        var visitor = new SelectionExpressionVisitor( FirstQuery, FollowingQueries.Count + 1 );

        foreach ( var selection in FirstQuery.Selection )
        {
            visitor.Selection = selection;
            selection.VisitExpressions( visitor );
        }

        foreach ( var followingQuery in FollowingQueries )
        {
            ++visitor.QueryIndex;
            foreach ( var selection in followingQuery.Query.Selection )
            {
                visitor.Selection = selection;
                selection.VisitExpressions( visitor );
            }
        }

        return visitor.GetSelection();
    }

    private sealed class SelectionExpressionVisitor : ISqlSelectNodeExpressionVisitor
    {
        private readonly int _queryCount;
        private readonly Dictionary<string, List<SqlSelectCompoundFieldNode.Origin>> _origins;

        internal SelectionExpressionVisitor(SqlQueryExpressionNode firstQuery, int queryCount)
        {
            QueryIndex = 0;
            _queryCount = queryCount;
            _origins = new Dictionary<string, List<SqlSelectCompoundFieldNode.Origin>>(
                capacity: firstQuery.Selection.Count,
                comparer: SqlHelpers.NameComparer );
        }

        internal int QueryIndex { get; set; }
        internal SqlSelectNode? Selection { get; set; }

        public void Handle(string name, SqlExpressionNode? expression)
        {
            Assume.IsNotNull( Selection );

            ref var origins = ref CollectionsMarshal.GetValueRefOrAddDefault( _origins, name, out var exists )!;
            if ( ! exists )
                origins = new List<SqlSelectCompoundFieldNode.Origin>( capacity: _queryCount );

            origins.Add( new SqlSelectCompoundFieldNode.Origin( QueryIndex, Selection, expression ) );
        }

        [Pure]
        internal ReadOnlyArray<SqlSelectNode> GetSelection()
        {
            if ( _origins.Count == 0 )
                return ReadOnlyArray<SqlSelectNode>.Empty;

            var index = 0;
            var result = new SqlSelectCompoundFieldNode[_origins.Count];
            foreach ( var (name, origin) in _origins )
                result[index++] = new SqlSelectCompoundFieldNode( name, origin.ToArray() );

            return result;
        }
    }
}
