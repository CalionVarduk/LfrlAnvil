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

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="SqlQueryExpressionNode"/> instance.
/// </summary>
public sealed class SqlQueryRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlQueryRecordSetNode(SqlQueryExpressionNode query, string alias, bool isOptional)
        : base( SqlNodeType.QueryRecordSet, alias, isOptional )
    {
        _info = SqlRecordSetInfo.Create( alias );
        Query = query;
        _fields = null;
    }

    /// <summary>
    /// Underlying <see cref="SqlQueryExpressionNode"/> instance.
    /// </summary>
    public SqlQueryExpressionNode Query { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => _info;

    /// <summary>
    /// Alias of this record set.
    /// </summary>
    public new string Alias
    {
        get
        {
            Assume.IsNotNull( base.Alias );
            return base.Alias;
        }
    }

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= Query.ExtractKnownDataFields( this );
        return _fields.Values;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= Query.ExtractKnownDataFields( this );
        return _fields.TryGetValue( name, out var field ) ? field : new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= Query.ExtractKnownDataFields( this );
        return _fields[name];
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryRecordSetNode As(string alias)
    {
        return new SqlQueryRecordSetNode( Query, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryRecordSetNode AsSelf()
    {
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlQueryRecordSetNode( Query, Alias, isOptional: optional )
            : this;
    }
}
