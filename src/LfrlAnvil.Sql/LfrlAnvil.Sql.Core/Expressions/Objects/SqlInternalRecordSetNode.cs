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

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on another record set.
/// </summary>
public sealed class SqlInternalRecordSetNode : SqlRecordSetNode
{
    internal SqlInternalRecordSetNode(SqlRecordSetNode @base)
        : base( SqlNodeType.Unknown, alias: null, isOptional: @base.IsOptional )
    {
        Base = @base;
        Info = SqlRecordSetInfo.Create( "<internal>" );
    }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Underlying <see cref="SqlRecordSetNode"/> instance.
    /// </summary>
    public SqlRecordSetNode Base { get; }

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlDataFieldNode> GetKnownFields()
    {
        var knownFields = Base.GetKnownFields();
        if ( knownFields.Count == 0 )
            return knownFields;

        var i = 0;
        var result = new SqlDataFieldNode[knownFields.Count];
        foreach ( var field in knownFields )
            result[i++] = field.ReplaceRecordSet( this );

        return result;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return Base.GetUnsafeField( name ).ReplaceRecordSet( this );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetField(string name)
    {
        return Base.GetField( name ).ReplaceRecordSet( this );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlInternalRecordSetNode MarkAsOptional(bool optional = true)
    {
        return IsOptional == optional ? this : new SqlInternalRecordSetNode( Base.MarkAsOptional( optional ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlInternalRecordSetNode AsSelf()
    {
        return this;
    }

    /// <inheritdoc />
    /// <exception cref="NotSupportedException">Aliased <see cref="SqlInternalRecordSetNode"/> instances are not supported.</exception>
    [Pure]
    public override SqlRecordSetNode As(string alias)
    {
        throw new NotSupportedException( ExceptionResources.InternalRecordSetsCannotBeAliased );
    }
}
