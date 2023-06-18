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
    public override SqlExpressionType? Type => null;

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

        var converter = new SelectionConverter( FirstQuery.Selection.Length, ignoreTypes );
        foreach ( var selection in FirstQuery.Selection.Span )
            selection.Convert( converter );

        foreach ( var followingQuery in FollowingQueries.Span )
        {
            foreach ( var selection in followingQuery.Query.Selection.Span )
                selection.Convert( converter );
        }

        var result = converter.Selection.Count > 0 ? new SqlSelectNode[converter.Selection.Count] : Array.Empty<SqlSelectNode>();
        converter.Selection.CopyTo( result );
        return result;
    }

    private sealed class SelectionConverter : ISqlSelectNodeConverter
    {
        private readonly bool _ignoreTypes;
        private readonly Dictionary<string, (SqlSelectNode Node, int Index)> _map;

        internal SelectionConverter(int capacity, bool ignoreTypes)
        {
            _ignoreTypes = ignoreTypes;
            Selection = new List<SqlSelectNode>( capacity: capacity );
            _map = new Dictionary<string, (SqlSelectNode Node, int Index)>(
                capacity: capacity,
                comparer: StringComparer.OrdinalIgnoreCase );
        }

        internal List<SqlSelectNode> Selection { get; }

        public void Add(string name, SqlExpressionType? type)
        {
            ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, name, out var exists );
            if ( ! exists )
            {
                entry = (SqlNode.RawSelect( name, alias: null, type: _ignoreTypes ? null : type ), Selection.Count);
                Selection.Add( entry.Node );
                return;
            }

            if ( _ignoreTypes )
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
