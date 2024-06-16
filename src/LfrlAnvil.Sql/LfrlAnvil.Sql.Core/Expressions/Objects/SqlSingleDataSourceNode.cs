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
/// Represents an SQL syntax tree node that defines a single data source from a single <see cref="SqlRecordSetNode"/> instance.
/// </summary>
/// <typeparam name="TRecordSetNode">SQL record set node type.</typeparam>
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

    /// <inheritdoc />
    public override TRecordSetNode From => _from[0];

    /// <inheritdoc />
    public override ReadOnlyArray<SqlDataSourceJoinOnNode> Joins => ReadOnlyArray<SqlDataSourceJoinOnNode>.Empty;

    /// <inheritdoc />
    public override IReadOnlyCollection<SqlRecordSetNode> RecordSets => _from;

    /// <inheritdoc cref="SqlDataSourceNode.this[string]" />
    public new TRecordSetNode this[string identifier] => GetRecordSet( identifier );

    /// <inheritdoc />
    [Pure]
    public override TRecordSetNode GetRecordSet(string identifier)
    {
        return identifier.Equals( From.Identifier, StringComparison.OrdinalIgnoreCase )
            ? From
            : throw new KeyNotFoundException( ExceptionResources.GivenRecordSetWasNotPresentInDataSource( identifier ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlSingleDataSourceNode<TRecordSetNode> AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlSingleDataSourceNode<TRecordSetNode> SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlSingleDataSourceNode<TRecordSetNode>( this, traits );
    }
}
