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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set based on an <see cref="ISqlViewBuilder"/> instance.
/// </summary>
public sealed class SqlViewBuilderNode : SqlRecordSetNode
{
    private readonly FieldCollection _fields;

    internal SqlViewBuilderNode(ISqlViewBuilder view, string? alias, bool isOptional)
        : base( SqlNodeType.ViewBuilder, alias, isOptional )
    {
        View = view;
        _fields = new FieldCollection( this );
    }

    /// <summary>
    /// Underlying <see cref="ISqlViewBuilder"/> instance.
    /// </summary>
    public ISqlViewBuilder View { get; }

    /// <inheritdoc />
    public override SqlRecordSetInfo Info => View.Info;

    /// <inheritdoc cref="SqlRecordSetNode.this[string]" />
    public new SqlQueryDataFieldNode this[string fieldName] => GetField( fieldName );

    /// <inheritdoc />
    [Pure]
    public override IReadOnlyCollection<SqlQueryDataFieldNode> GetKnownFields()
    {
        return _fields;
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewBuilderNode As(string alias)
    {
        return new SqlViewBuilderNode( View, alias, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewBuilderNode AsSelf()
    {
        return new SqlViewBuilderNode( View, alias: null, IsOptional );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDataFieldNode GetUnsafeField(string name)
    {
        return ( SqlDataFieldNode? )_fields.TryGet( name ) ?? new SqlRawDataFieldNode( this, name, type: null );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlQueryDataFieldNode GetField(string name)
    {
        return _fields.Get( name );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlViewBuilderNode MarkAsOptional(bool optional = true)
    {
        return IsOptional != optional
            ? new SqlViewBuilderNode( View, Alias, isOptional: optional )
            : this;
    }

    private sealed class FieldCollection : IReadOnlyCollection<SqlQueryDataFieldNode>
    {
        private readonly SqlViewBuilderNode _owner;

        internal FieldCollection(SqlViewBuilderNode owner)
        {
            _owner = owner;
        }

        public int Count => _owner.View.Source.ExtractKnownDataFieldCount();

        [Pure]
        public IEnumerator<SqlQueryDataFieldNode> GetEnumerator()
        {
            return _owner.View.Source.ExtractKnownDataFields( _owner ).Values.GetEnumerator();
        }

        [Pure]
        internal SqlQueryDataFieldNode Get(string name)
        {
            var info = _owner.View.Source.TryFindKnownDataFieldInfo( name );
            if ( info is null )
                throw new KeyNotFoundException( ExceptionResources.FieldDoesNotExist( name ) );

            return GetNode( name, info.Value );
        }

        [Pure]
        internal SqlQueryDataFieldNode? TryGet(string name)
        {
            var info = _owner.View.Source.TryFindKnownDataFieldInfo( name );
            return info is not null ? GetNode( name, info.Value ) : null;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private SqlQueryDataFieldNode GetNode(string name, SqlQueryExpressionNode.KnownDataFieldInfo info)
        {
            return new SqlQueryDataFieldNode( _owner, name, info.Selection, info.Expression );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
