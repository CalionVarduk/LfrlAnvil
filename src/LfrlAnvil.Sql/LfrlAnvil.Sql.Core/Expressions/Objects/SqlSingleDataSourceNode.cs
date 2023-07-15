using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlSingleDataSourceNode<TRecordSetNode> : SqlDataSourceNode
    where TRecordSetNode : SqlRecordSetNode
{
    private readonly TRecordSetNode[] _from;

    internal SqlSingleDataSourceNode(TRecordSetNode from)
        : base( Chain<SqlDataSourceTraitNode>.Empty )
    {
        _from = new[] { from };
    }

    private SqlSingleDataSourceNode(SqlSingleDataSourceNode<TRecordSetNode> @base, Chain<SqlDataSourceTraitNode> traits)
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
    public override SqlSingleDataSourceNode<TRecordSetNode> AddTrait(SqlDataSourceTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlSingleDataSourceNode<TRecordSetNode>( this, traits );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder.Append( "FROM" ).Append( ' ' ), From, indent );

        foreach ( var trait in Traits )
            AppendTo( builder.Indent( indent ), trait, indent );
    }
}
