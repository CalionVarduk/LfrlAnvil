// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
