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
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="SqlCreateTableNode"/> instance.
/// </summary>
public sealed class SqlNewTableNode : SqlRecordSetNode
{
    private Dictionary<string, SqlRawDataFieldNode>? _columns;

    internal SqlNewTableNode(SqlCreateTableNode creationNode, string? alias, bool isOptional)
        : base( SqlNodeType.NewTable, alias, isOptional )
    {
        CreationNode = creationNode;
        _columns = null;
    }

    /// <summary>
    /// Underlying <see cref="SqlCreateTableNode"/> instance.
    /// </summary>
    public SqlCreateTableNode CreationNode { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => CreationNode.Info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlRawDataFieldNode> GetKnownFields()
    {
        _columns ??= CreateColumnFields();
        return _columns.Values;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNewTableNode As(string alias)
    {
        return new SqlNewTableNode( CreationNode, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNewTableNode AsSelf()
    {
        return new SqlNewTableNode( CreationNode, alias: null, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns.TryGetValue( name, out var column ) ? column : new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRawDataFieldNode GetField(string name)
    {
        _columns ??= CreateColumnFields();
        return _columns[name];
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNewTableNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNewTableNode( CreationNode, alias: Alias, isOptional: optional )
            : this;
    }

    [Pure]
    private Dictionary<string, SqlRawDataFieldNode> CreateColumnFields()
    {
        var columns = CreationNode.Columns;
        var result = new Dictionary<string, SqlRawDataFieldNode>( capacity: columns.Count, comparer: SqlHelpers.NameComparer );

        foreach ( var column in columns )
            result.Add( column.Name, new SqlRawDataFieldNode( this, column.Name, IsOptional ? column.Type.MakeNullable() : column.Type ) );

        return result;
    }
}
