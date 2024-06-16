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
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="SqlCreateViewNode"/> instance.
/// </summary>
public sealed class SqlNewViewNode : SqlRecordSetNode
{
    private Dictionary<string, SqlQueryDataFieldNode>? _fields;

    internal SqlNewViewNode(SqlCreateViewNode creationNode, string? alias, bool isOptional)
        : base( SqlNodeType.NewView, alias, isOptional )
    {
        CreationNode = creationNode;
        _fields = null;
    }

    /// <summary>
    /// Underlying <see cref="SqlCreateViewNode"/> instance.
    /// </summary>
    public SqlCreateViewNode CreationNode { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => CreationNode.Info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        _fields ??= CreationNode.Source.ExtractKnownDataFields( this );
        return _fields.Values;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNewViewNode As(string alias)
    {
        return new SqlNewViewNode( CreationNode, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNewViewNode AsSelf()
    {
        return new SqlNewViewNode( CreationNode, alias: null, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _fields ??= CreationNode.Source.ExtractKnownDataFields( this );
        return _fields.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        _fields ??= CreationNode.Source.ExtractKnownDataFields( this );
        return _fields[name];
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNewViewNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNewViewNode( CreationNode, alias: Alias, isOptional: optional )
            : this;
    }
}
