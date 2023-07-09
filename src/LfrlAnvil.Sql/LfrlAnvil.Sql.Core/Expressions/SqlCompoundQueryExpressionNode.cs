using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCompoundQueryExpressionNode : SqlExtendableQueryExpressionNode
{
    private ReadOnlyMemory<SqlSelectNode>? _selection;

    internal SqlCompoundQueryExpressionNode(
        SqlQueryExpressionNode firstQuery,
        SqlCompoundQueryComponentNode[] followingQueries)
        : base( SqlNodeType.CompoundQuery, Chain<SqlQueryDecoratorNode>.Empty )
    {
        Ensure.IsNotEmpty( followingQueries, nameof( followingQueries ) );
        FirstQuery = firstQuery;
        FollowingQueries = followingQueries;
        _selection = null;
    }

    private SqlCompoundQueryExpressionNode(SqlCompoundQueryExpressionNode @base, Chain<SqlQueryDecoratorNode> decorators)
        : base( SqlNodeType.CompoundQuery, decorators )
    {
        FirstQuery = @base.FirstQuery;
        FollowingQueries = @base.FollowingQueries;
        _selection = @base._selection;
    }

    public SqlQueryExpressionNode FirstQuery { get; }
    public ReadOnlyMemory<SqlCompoundQueryComponentNode> FollowingQueries { get; }
    public override ReadOnlyMemory<SqlSelectNode> Selection => _selection ??= CreateSelection();

    [Pure]
    public override SqlCompoundQueryExpressionNode Decorate(SqlQueryDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlCompoundQueryExpressionNode( this, decorators );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var elementIndent = indent + DefaultIndent;

        builder.Append( '(' ).Indent( elementIndent );
        AppendTo( builder, FirstQuery, elementIndent );
        builder.Indent( indent ).Append( ')' );

        foreach ( var followingQuery in FollowingQueries.Span )
            AppendTo( builder.Indent( indent ), followingQuery, indent );

        foreach ( var decorator in Decorators )
            AppendTo( builder.Indent( indent ), decorator, indent );
    }

    [Pure]
    private ReadOnlyMemory<SqlSelectNode> CreateSelection()
    {
        var converter = new SelectionConverter( FirstQuery, FollowingQueries.Length + 1 );

        foreach ( var selection in FirstQuery.Selection.Span )
        {
            converter.Selection = selection;
            selection.Convert( converter );
        }

        foreach ( var followingQuery in FollowingQueries.Span )
        {
            ++converter.QueryIndex;
            foreach ( var selection in followingQuery.Query.Selection.Span )
            {
                converter.Selection = selection;
                selection.Convert( converter );
            }
        }

        return converter.GetSelection();
    }

    private sealed class SelectionConverter : ISqlSelectNodeConverter
    {
        private readonly int _queryCount;
        private readonly Dictionary<string, List<SqlSelectCompoundFieldNode.Origin>> _origins;

        internal SelectionConverter(SqlQueryExpressionNode firstQuery, int queryCount)
        {
            QueryIndex = 0;
            _queryCount = queryCount;
            _origins = new Dictionary<string, List<SqlSelectCompoundFieldNode.Origin>>(
                capacity: firstQuery.Selection.Length,
                comparer: StringComparer.OrdinalIgnoreCase );
        }

        internal int QueryIndex { get; set; }
        internal SqlSelectNode? Selection { get; set; }

        public void Add(string name, SqlExpressionNode? expression)
        {
            Assume.IsNotNull( Selection, nameof( Selection ) );

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
