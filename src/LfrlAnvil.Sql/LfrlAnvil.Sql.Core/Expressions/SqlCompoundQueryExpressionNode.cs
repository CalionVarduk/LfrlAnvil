using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCompoundQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    private ReadOnlyMemory<SqlSelectNode>? _selection;

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

    public SqlQueryExpressionNode FirstQuery { get; }
    public ReadOnlyMemory<SqlCompoundQueryComponentNode> FollowingQueries { get; }
    public override ReadOnlyMemory<SqlSelectNode> Selection => _selection ??= CreateSelection();

    [Pure]
    public override SqlCompoundQueryExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlCompoundQueryExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlCompoundQueryExpressionNode( this, traits );
    }

    [Pure]
    private ReadOnlyMemory<SqlSelectNode> CreateSelection()
    {
        var visitor = new SelectionExpressionVisitor( FirstQuery, FollowingQueries.Length + 1 );

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
                capacity: firstQuery.Selection.Length,
                comparer: StringComparer.OrdinalIgnoreCase );
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
        internal ReadOnlyMemory<SqlSelectNode> GetSelection()
        {
            if ( _origins.Count == 0 )
                return ReadOnlyMemory<SqlSelectNode>.Empty;

            var index = 0;
            var result = new SqlSelectCompoundFieldNode[_origins.Count];
            foreach ( var (name, origin) in _origins )
                result[index++] = new SqlSelectCompoundFieldNode( name, origin.ToArray() );

            return result;
        }
    }
}
