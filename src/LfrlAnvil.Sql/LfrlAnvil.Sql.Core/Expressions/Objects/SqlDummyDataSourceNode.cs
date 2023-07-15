using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlDummyDataSourceNode : SqlDataSourceNode
{
    internal SqlDummyDataSourceNode(Chain<SqlDataSourceTraitNode> traits)
        : base( traits ) { }

    public override SqlRecordSetNode From =>
        throw new InvalidOperationException( ExceptionResources.DummyDataSourceDoesNotContainAnyRecordSets );

    public override ReadOnlyMemory<SqlDataSourceJoinOnNode> Joins => ReadOnlyMemory<SqlDataSourceJoinOnNode>.Empty;
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => Array.Empty<SqlRecordSetNode>();

    [Pure]
    public override SqlRecordSetNode GetRecordSet(string name)
    {
        throw new InvalidOperationException( ExceptionResources.DummyDataSourceDoesNotContainAnyRecordSets );
    }

    [Pure]
    public override SqlDummyDataSourceNode AddTrait(SqlDataSourceTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlDummyDataSourceNode( traits );
    }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "FROM" ).Append( ' ' ).Append( '<' ).Append( "DUMMY" ).Append( '>' );

        foreach ( var trait in Traits )
            AppendTo( builder.Indent( indent ), trait, indent );
    }
}
