﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlDummyDataSourceNode : SqlDataSourceNode
{
    internal SqlDummyDataSourceNode(Chain<SqlTraitNode> traits)
        : base( traits ) { }

    public override SqlRecordSetNode From =>
        throw new InvalidOperationException( ExceptionResources.DummyDataSourceDoesNotContainAnyRecordSets );

    public override ReadOnlyArray<SqlDataSourceJoinOnNode> Joins => ReadOnlyArray<SqlDataSourceJoinOnNode>.Empty;
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => Array.Empty<SqlRecordSetNode>();

    [Pure]
    public override SqlRecordSetNode GetRecordSet(string name)
    {
        throw new InvalidOperationException( ExceptionResources.DummyDataSourceDoesNotContainAnyRecordSets );
    }

    [Pure]
    public override SqlDummyDataSourceNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlDummyDataSourceNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlDummyDataSourceNode( traits );
    }
}
