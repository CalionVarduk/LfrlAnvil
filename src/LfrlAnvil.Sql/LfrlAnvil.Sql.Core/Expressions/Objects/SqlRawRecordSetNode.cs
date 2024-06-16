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

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set without a source.
/// </summary>
public class SqlRawRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    /// <summary>
    /// Creates a new <see cref="SqlRawRecordSetNode"/> instance marked as raw.
    /// </summary>
    /// <param name="name">Raw name of this record set.</param>
    /// <param name="alias">Optional alias of this record set.</param>
    /// <param name="isOptional">Specifies whether or not this record set is marked as optional.</param>
    protected internal SqlRawRecordSetNode(string name, string? alias, bool isOptional)
        : this( SqlRecordSetInfo.Create( name ), alias, isOptional, isInfoRaw: true ) { }

    /// <summary>
    /// Creates a new <see cref="SqlRawRecordSetNode"/> instance.
    /// </summary>
    /// <param name="info"><see cref="SqlRecordSetInfo"/> associated with this record set.</param>
    /// <param name="alias">Optional alias of this record set.</param>
    /// <param name="isOptional">Specifies whether or not this record set is marked as optional.</param>
    protected internal SqlRawRecordSetNode(SqlRecordSetInfo info, string? alias, bool isOptional)
        : this( info, alias, isOptional, isInfoRaw: false ) { }

    private SqlRawRecordSetNode(SqlRecordSetInfo info, string? alias, bool isOptional, bool isInfoRaw)
        : base( SqlNodeType.RawRecordSet, alias, isOptional )
    {
        _info = info;
        IsInfoRaw = isInfoRaw;
    }

    /// <inheritdoc />
    public sealed override SqlRecordSetInfo Info => _info;

    /// <summary>
    /// Specifies whether or not this record set has been created with a <see cref="String"/> name
    /// rather than <see cref="SqlRecordSetInfo"/>.
    /// </summary>
    public bool IsInfoRaw { get; }

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawRecordSetNode As(string alias)
    {
        return new SqlRawRecordSetNode( Info, alias, IsOptional, IsInfoRaw );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawRecordSetNode AsSelf()
    {
        return new SqlRawRecordSetNode( Info, alias: null, IsOptional, IsInfoRaw );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawDataFieldNode GetUnsafeField(string name)
    {
        return new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawDataFieldNode GetField(string name)
    {
        return new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlRawRecordSetNode( Info, alias: Alias, isOptional: optional, IsInfoRaw )
            : this;
    }
}
