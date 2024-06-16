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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlIndex" />
public abstract class SqlIndex : SqlConstraint, ISqlIndex
{
    private readonly ReadOnlyArray<SqlIndexed<ISqlColumn>> _columns;

    /// <summary>
    /// Creates a new <see cref="SqlIndex"/> instance.
    /// </summary>
    /// <param name="table">Table that this index belongs to.</param>
    /// <param name="builder">Source builder.</param>
    protected SqlIndex(SqlTable table, SqlIndexBuilder builder)
        : base( table, builder )
    {
        IsUnique = builder.IsUnique;
        IsPartial = builder.Filter is not null;
        IsVirtual = builder.IsVirtual;

        var columns = new SqlIndexed<ISqlColumn>[builder.Columns.Expressions.Count];
        for ( var i = 0; i < columns.Length; ++i )
        {
            var column = builder.Columns.TryGet( i );
            columns[i] = new SqlIndexed<ISqlColumn>(
                column is null ? null : table.Columns.Get( column.Name ),
                builder.Columns.Expressions[i].Ordering );
        }

        _columns = columns;
    }

    /// <inheritdoc />
    public bool IsUnique { get; }

    /// <inheritdoc />
    public bool IsPartial { get; }

    /// <inheritdoc />
    public bool IsVirtual { get; }

    /// <inheritdoc cref="ISqlIndex.Columns" />
    public SqlIndexedArray<SqlColumn> Columns => SqlIndexedArray<SqlColumn>.From( _columns );

    IReadOnlyList<SqlIndexed<ISqlColumn>> ISqlIndex.Columns => _columns.GetUnderlyingArray();
}
