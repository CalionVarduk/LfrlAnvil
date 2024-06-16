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
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteTable : SqlTable
{
    internal SqliteTable(SqliteSchema schema, SqliteTableBuilder builder)
        : base( schema, builder, new SqliteColumnCollection( builder.Columns ), new SqliteConstraintCollection( builder.Constraints ) ) { }

    /// <inheritdoc cref="SqlTable.Columns" />
    public new SqliteColumnCollection Columns => ReinterpretCast.To<SqliteColumnCollection>( base.Columns );

    /// <inheritdoc cref="SqlTable.Constraints" />
    public new SqliteConstraintCollection Constraints => ReinterpretCast.To<SqliteConstraintCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTable.Schema" />
    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteTable"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }
}
