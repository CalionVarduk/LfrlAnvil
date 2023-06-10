using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlSingleDataSourceNode<TRecordSetNode> : SqlDataSourceNode
    where TRecordSetNode : SqlRecordSetNode
{
    private readonly TRecordSetNode[] _from;

    internal SqlSingleDataSourceNode(TRecordSetNode from)
    {
        _from = new[] { from };
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

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendTo( builder.Append( "FROM" ).Append( ' ' ), From, indent );
    }
}
