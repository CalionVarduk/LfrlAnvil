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
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlTable" />
public abstract class SqlTable : SqlObject, ISqlTable
{
    private SqlTableNode? _node;
    private SqlRecordSetInfo? _info;

    /// <summary>
    /// Creates a new <see cref="SqlTable"/> instance.
    /// </summary>
    /// <param name="schema">Schema that this table belongs to.</param>
    /// <param name="builder">Source builder.</param>
    /// <param name="columns">Collection of columns that belong to this table.</param>
    /// <param name="constraints">Collection of constraints that belong to this table.</param>
    protected SqlTable(SqlSchema schema, SqlTableBuilder builder, SqlColumnCollection columns, SqlConstraintCollection constraints)
        : base( schema.Database, builder )
    {
        Schema = schema;
        _info = builder.GetCachedInfo();
        _node = null;
        Columns = columns;
        Columns.SetTable( this, builder.Columns );
        Constraints = constraints;
        Constraints.SetTable( this );
    }

    /// <inheritdoc cref="ISqlTable.Schema" />
    public SqlSchema Schema { get; }

    /// <inheritdoc cref="ISqlTable.Columns" />
    public SqlColumnCollection Columns { get; }

    /// <inheritdoc cref="ISqlTable.Constraints" />
    public SqlConstraintCollection Constraints { get; }

    /// <inheritdoc />
    public SqlRecordSetInfo Info => _info ??= SqlRecordSetInfo.Create( Schema.Name, Name );

    /// <inheritdoc />
    public SqlTableNode Node => _node ??= SqlNode.Table( this );

    ISqlSchema ISqlTable.Schema => Schema;
    ISqlColumnCollection ISqlTable.Columns => Columns;
    ISqlConstraintCollection ISqlTable.Constraints => Constraints;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlTable"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( Schema.Name, Name )}";
    }
}
