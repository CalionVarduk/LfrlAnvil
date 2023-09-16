using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlSingleDataSourceNode<TRecordSetNode> : SqlDataSourceNode
    where TRecordSetNode : SqlRecordSetNode
{
    private readonly TRecordSetNode[] _from;

    internal SqlSingleDataSourceNode(TRecordSetNode from)
        : base( Chain<SqlTraitNode>.Empty )
    {
        _from = new[] { from };
    }

    private SqlSingleDataSourceNode(SqlSingleDataSourceNode<TRecordSetNode> @base, Chain<SqlTraitNode> traits)
        : base( traits )
    {
        _from = @base._from;
    }

    public override TRecordSetNode From => _from[0];
    public override ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins => ReadOnlyMemory<SqlDataSourceJoinOnNode>.Empty;
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => _from;
    public new TRecordSetNode this[string name] => GetRecordSet( name );

    [Pure]
    public override TRecordSetNode GetRecordSet(string name)
    {
        return name.Equals( From.Name, StringComparison.OrdinalIgnoreCase )
            ? From
            : throw new KeyNotFoundException( ExceptionResources.GivenRecordSetWasNotPresentInDataSource( name ) );
    }

    [Pure]
    public override SqlSingleDataSourceNode<TRecordSetNode> AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlSingleDataSourceNode<TRecordSetNode>( this, traits );
    }
}
