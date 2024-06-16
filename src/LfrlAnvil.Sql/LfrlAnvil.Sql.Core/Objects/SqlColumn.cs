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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlColumn" />
public abstract class SqlColumn : SqlObject, ISqlColumn
{
    private SqlColumnNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlColumn"/> instance.
    /// </summary>
    /// <param name="table">Table that this column belongs to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlColumn(SqlTable table, SqlColumnBuilder builder)
        : base( table.Database, builder )
    {
        Table = table;
        IsNullable = builder.IsNullable;
        HasDefaultValue = builder.DefaultValue is not null;
        ComputationStorage = builder.Computation?.Storage;
        TypeDefinition = builder.TypeDefinition;
        _node = null;
    }

    /// <inheritdoc cref="ISqlColumn.Table" />
    public SqlTable Table { get; }

    /// <inheritdoc />
    public bool IsNullable { get; }

    /// <inheritdoc />
    public bool HasDefaultValue { get; }

    /// <inheritdoc />
    public SqlColumnComputationStorage? ComputationStorage { get; }

    /// <inheritdoc cref="ISqlColumn.TypeDefinition" />
    public SqlColumnTypeDefinition TypeDefinition { get; }

    /// <inheritdoc />
    public SqlColumnNode Node => _node ??= Table.Node[Name];

    ISqlTable ISqlColumn.Table => Table;
    ISqlColumnTypeDefinition ISqlColumn.TypeDefinition => TypeDefinition;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlColumn"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }

    /// <inheritdoc />
    [Pure]
    public SqlOrderByNode Asc()
    {
        return SqlNode.OrderByAsc( Node );
    }

    /// <inheritdoc />
    [Pure]
    public SqlOrderByNode Desc()
    {
        return SqlNode.OrderByDesc( Node );
    }
}
