using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single dummy data source, that is a data source that does not contain any record sets.
/// </summary>
public sealed class SqlDummyDataSourceNode : SqlDataSourceNode
{
    internal SqlDummyDataSourceNode(Chain<SqlTraitNode> traits)
        : base( traits ) { }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Dummy data source does not contain any record sets.</exception>
    public override SqlRecordSetNode From =>
        throw new InvalidOperationException( ExceptionResources.DummyDataSourceDoesNotContainAnyRecordSets );

    /// <inheritdoc />
    public override ReadOnlyArray<SqlDataSourceJoinOnNode> Joins => ReadOnlyArray<SqlDataSourceJoinOnNode>.Empty;

    /// <inheritdoc />
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => Array.Empty<SqlRecordSetNode>();

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">Dummy data source does not contain any record sets.</exception>
    [Pure]
    public override SqlRecordSetNode GetRecordSet(string identifier)
    {
        throw new InvalidOperationException( ExceptionResources.DummyDataSourceDoesNotContainAnyRecordSets );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDummyDataSourceNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDummyDataSourceNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlDummyDataSourceNode( traits );
    }
}
