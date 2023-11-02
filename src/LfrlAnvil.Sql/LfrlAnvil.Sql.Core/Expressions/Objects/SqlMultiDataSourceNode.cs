using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public class SqlMultiDataSourceNode : SqlDataSourceNode
{
    private readonly Dictionary<string, SqlRecordSetNode> _recordSets;

    protected internal SqlMultiDataSourceNode(SqlRecordSetNode from, SqlDataSourceJoinOnNode[] joins)
        : base( Chain<SqlTraitNode>.Empty )
    {
        Joins = joins;
        from = from.MarkAsOptional( false );
        _recordSets = CreateRecordSetDictionary( joins.Length + 1 );
        _recordSets.Add( from.Identifier, from );

        var lastRightJoinIndex = -1;
        for ( var i = 0; i < joins.Length; ++i )
            lastRightJoinIndex = AddNextJoin( from, _recordSets, joins, lastRightJoinIndex, i );

        From = _recordSets[from.Identifier];
        Joins = joins;
    }

    protected internal SqlMultiDataSourceNode(SqlRecordSetNode from, SqlJoinDefinition[] definitions)
        : base( Chain<SqlTraitNode>.Empty )
    {
        from = from.MarkAsOptional( false );
        _recordSets = CreateRecordSetDictionary( definitions.Length + 1 );
        _recordSets.Add( from.Identifier, from );

        if ( definitions.Length == 0 )
        {
            From = from;
            Joins = ReadOnlyMemory<SqlDataSourceJoinOnNode>.Empty;
            return;
        }

        var joins = new SqlDataSourceJoinOnNode[definitions.Length];

        var lastRightJoinIndex = -1;
        for ( var i = 0; i < definitions.Length; ++i )
        {
            var definition = definitions[i];
            var onExpression = definition.OnExpression( new SqlJoinDefinition.ExpressionParams( _recordSets, definition.InnerRecordSet ) );
            joins[i] = new SqlDataSourceJoinOnNode( definition.JoinType, definition.InnerRecordSet, onExpression );
            lastRightJoinIndex = AddNextJoin( from, _recordSets, joins, lastRightJoinIndex, i );
        }

        From = _recordSets[from.Identifier];
        Joins = joins;
    }

    internal SqlMultiDataSourceNode(SqlDataSourceNode source, SqlDataSourceJoinOnNode[] newJoins)
        : base( source.Traits )
    {
        var from = source.From;
        var sourceRecordSets = source.RecordSets;
        _recordSets = CreateRecordSetDictionary( sourceRecordSets.Count );
        foreach ( var recordSet in sourceRecordSets )
            _recordSets.Add( recordSet.Identifier, recordSet );

        var sourceJoins = source.Joins;
        var offset = sourceJoins.Length;
        SqlDataSourceJoinOnNode[] joins;
        if ( offset > 0 )
        {
            joins = new SqlDataSourceJoinOnNode[offset + newJoins.Length];
            sourceJoins.CopyTo( joins );
            newJoins.CopyTo( joins, offset );
        }
        else
            joins = newJoins;

        var lastRightJoinIndex = -1;
        for ( var i = offset; i < joins.Length; ++i )
            lastRightJoinIndex = AddNextJoin( from, _recordSets, joins, lastRightJoinIndex, i );

        From = _recordSets[from.Identifier];
        Joins = joins;
    }

    internal SqlMultiDataSourceNode(SqlDataSourceNode source, SqlJoinDefinition[] newDefinitions)
        : base( source.Traits )
    {
        var from = source.From;
        var sourceRecordSets = source.RecordSets;
        _recordSets = CreateRecordSetDictionary( sourceRecordSets.Count );
        foreach ( var recordSet in sourceRecordSets )
            _recordSets.Add( recordSet.Identifier, recordSet );

        var sourceJoins = source.Joins;
        if ( newDefinitions.Length == 0 )
        {
            From = from;
            Joins = sourceJoins;
            return;
        }

        var offset = sourceJoins.Length;
        SqlDataSourceJoinOnNode[] joins;
        if ( offset > 0 )
        {
            joins = new SqlDataSourceJoinOnNode[offset + newDefinitions.Length];
            sourceJoins.CopyTo( joins );
        }
        else
            joins = new SqlDataSourceJoinOnNode[newDefinitions.Length];

        var lastRightJoinIndex = -1;
        for ( var i = 0; i < newDefinitions.Length; ++i )
        {
            var index = i + offset;
            var definition = newDefinitions[i];
            var onExpression = definition.OnExpression( new SqlJoinDefinition.ExpressionParams( _recordSets, definition.InnerRecordSet ) );
            joins[index] = new SqlDataSourceJoinOnNode( definition.JoinType, definition.InnerRecordSet, onExpression );
            lastRightJoinIndex = AddNextJoin( from, _recordSets, joins, lastRightJoinIndex, index );
        }

        From = _recordSets[from.Identifier];
        Joins = joins;
    }

    protected SqlMultiDataSourceNode(SqlMultiDataSourceNode @base, Chain<SqlTraitNode> traits)
        : base( traits )
    {
        From = @base.From;
        Joins = @base.Joins;
        _recordSets = @base._recordSets;
    }

    public sealed override SqlRecordSetNode From { get; }
    public sealed override ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins { get; }
    public sealed override IReadOnlyCollection<SqlRecordSetNode> RecordSets => _recordSets.Values;

    [Pure]
    public sealed override SqlRecordSetNode GetRecordSet(string name)
    {
        return _recordSets[name];
    }

    [Pure]
    public override SqlMultiDataSourceNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlMultiDataSourceNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlMultiDataSourceNode( this, traits );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static Dictionary<string, SqlRecordSetNode> CreateRecordSetDictionary(int capacity)
    {
        return new Dictionary<string, SqlRecordSetNode>( capacity: capacity, comparer: StringComparer.OrdinalIgnoreCase );
    }

    private static int AddNextJoin(
        SqlRecordSetNode from,
        Dictionary<string, SqlRecordSetNode> recordSets,
        SqlDataSourceJoinOnNode[] joins,
        int lastRightJoinIndex,
        int index)
    {
        var join = joins[index];
        var innerRecordSet = join.InnerRecordSet;

        switch ( join.JoinType )
        {
            case SqlJoinType.Left:
            {
                innerRecordSet = innerRecordSet.MarkAsOptional();
                break;
            }
            case SqlJoinType.Right:
            {
                innerRecordSet = innerRecordSet.MarkAsOptional( false );
                MarkRightJoinOuterSetsAsOptional( from, recordSets, joins, lastRightJoinIndex, index );
                lastRightJoinIndex = index;
                break;
            }
            case SqlJoinType.Full:
            {
                innerRecordSet = innerRecordSet.MarkAsOptional();
                MarkRightJoinOuterSetsAsOptional( from, recordSets, joins, lastRightJoinIndex, index );
                lastRightJoinIndex = index;
                break;
            }
            default:
            {
                innerRecordSet = innerRecordSet.MarkAsOptional( false );
                break;
            }
        }

        recordSets.Add( innerRecordSet.Identifier, innerRecordSet );
        return lastRightJoinIndex;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static void MarkRightJoinOuterSetsAsOptional(
            SqlRecordSetNode from,
            Dictionary<string, SqlRecordSetNode> recordSets,
            SqlDataSourceJoinOnNode[] joins,
            int lastRightJoinIndex,
            int index)
        {
            int startIndex;

            if ( lastRightJoinIndex == -1 )
            {
                startIndex = 0;
                recordSets[from.Identifier] = from.MarkAsOptional();
            }
            else
                startIndex = lastRightJoinIndex;

            var span = joins.Slice( startIndex, index - startIndex );
            foreach ( var join in span )
            {
                var outerRecordSet = join.InnerRecordSet;
                if ( ! outerRecordSet.IsOptional )
                    recordSets[outerRecordSet.Identifier] = outerRecordSet.MarkAsOptional();
            }
        }
    }
}
