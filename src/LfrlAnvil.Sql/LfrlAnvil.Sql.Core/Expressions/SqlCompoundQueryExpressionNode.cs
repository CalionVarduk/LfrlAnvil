using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCompoundQueryExpressionNode : SqlQueryExpressionNode
{
    private ReadOnlyMemory<SqlSelectNode>? _selection;

    internal SqlCompoundQueryExpressionNode(SqlQueryExpressionNode firstQuery, SqlCompoundQueryComponentNode[] followingQueries)
        : base( SqlNodeType.CompoundQuery )
    {
        Assume.IsNotEmpty( followingQueries, nameof( followingQueries ) );
        FirstQuery = firstQuery;
        FollowingQueries = followingQueries;
        _selection = null;
    }

    public SqlQueryExpressionNode FirstQuery { get; }
    public ReadOnlyMemory<SqlCompoundQueryComponentNode> FollowingQueries { get; }
    public override ReadOnlyMemory<SqlSelectNode> Selection => _selection ??= CreateSelection();
    public override SqlExpressionType? Type => null;

    protected override void ToString(StringBuilder builder, int indent)
    {
        var queryIndent = indent + DefaultIndent;
        builder.Append( '(' ).Indent( queryIndent );
        AppendTo( builder, FirstQuery, queryIndent );
        builder.Indent( indent ).Append( ')' );

        foreach ( var followingQuery in FollowingQueries.Span )
            AppendTo( builder.Indent( indent ), followingQuery, indent );
    }

    [Pure]
    private ReadOnlyMemory<SqlSelectNode> CreateSelection()
    {
        var ignoreTypes = FirstQuery.NodeType == SqlNodeType.RawQuery;
        if ( ! ignoreTypes )
        {
            foreach ( var followingQuery in FollowingQueries.Span )
            {
                ignoreTypes = followingQuery.Query.NodeType == SqlNodeType.RawQuery;
                if ( ignoreTypes )
                    break;
            }
        }

        var initializer = new SelectionInitializer( FirstQuery.Selection.Length, ignoreTypes );
        foreach ( var selection in FirstQuery.Selection.Span )
            selection.RegisterCompoundSelection( initializer );

        foreach ( var followingQuery in FollowingQueries.Span )
        {
            foreach ( var selection in followingQuery.Query.Selection.Span )
                selection.RegisterCompoundSelection( initializer );
        }

        var result = initializer.Selection.Count > 0 ? new SqlSelectNode[initializer.Selection.Count] : Array.Empty<SqlSelectNode>();
        initializer.Selection.CopyTo( result );
        return result;
    }

    public readonly struct SelectionInitializer
    {
        private readonly Dictionary<string, (SqlSelectNode Node, int Index)> _map;

        internal SelectionInitializer(int capacity, bool ignoreTypes)
        {
            IgnoreTypes = ignoreTypes;
            Selection = new List<SqlSelectNode>( capacity: capacity );
            _map = new Dictionary<string, (SqlSelectNode Node, int Index)>(
                capacity: capacity,
                comparer: StringComparer.OrdinalIgnoreCase );
        }

        public bool IgnoreTypes { get; }
        internal List<SqlSelectNode> Selection { get; }

        public void AddSelection(string name, SqlExpressionType? type)
        {
            ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists );
            if ( ! exists )
            {
                entry = (SqlNode.RawSelect( name, alias: null, type: IgnoreTypes ? null : type ), Selection.Count);
                Selection.Add( entry.Node );
                return;
            }

            if ( IgnoreTypes )
                return;

            var currentType = entry.Node.Type;
            var newType = SqlExpressionType.HaveCommonType( currentType, type )
                ? SqlExpressionType.GetCommonType( currentType, type )
                : null;

            if ( currentType == newType )
                return;

            entry = (SqlNode.RawSelect( name, alias: null, newType ), entry.Index);
            Selection[entry.Index] = entry.Node;
        }
    }
}
