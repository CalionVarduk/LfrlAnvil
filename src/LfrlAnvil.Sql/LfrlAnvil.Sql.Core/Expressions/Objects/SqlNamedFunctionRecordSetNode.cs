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
using LfrlAnvil.Sql.Expressions.Functions;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on a named table-valued function.
/// </summary>
public sealed class SqlNamedFunctionRecordSetNode : SqlRecordSetNode
{
    private readonly SqlRecordSetInfo _info;

    internal SqlNamedFunctionRecordSetNode(SqlNamedFunctionExpressionNode function, string alias, bool isOptional)
        : base( SqlNodeType.NamedFunctionRecordSet, alias, isOptional )
    {
        _info = SqlRecordSetInfo.Create( alias );
        Function = function;
    }

    /// <summary>
    /// Underlying <see cref="SqlNamedFunctionExpressionNode"/> instance.
    /// </summary>
    public SqlNamedFunctionExpressionNode Function { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => _info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlRawDataFieldNode this[string fieldName] => GetField( fieldName );

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

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        return Array.Empty<SqlDataFieldNode>();
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedFunctionRecordSetNode As(string alias)
    {
        return new SqlNamedFunctionRecordSetNode( Function, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedFunctionRecordSetNode AsSelf()
    {
        return this;
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
    public override SqlNamedFunctionRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlNamedFunctionRecordSetNode( Function, Alias, isOptional: optional )
            : this;
    }
}
