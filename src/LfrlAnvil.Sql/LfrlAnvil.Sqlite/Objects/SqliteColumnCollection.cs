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
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumnCollection : SqlColumnCollection
{
    internal SqliteColumnCollection(SqliteColumnBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlColumnCollection.Table" />
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    /// <inheritdoc cref="SqlColumnCollection.Get(string)" />
    [Pure]
    public new SqliteColumn Get(string name)
    {
        return ReinterpretCast.To<SqliteColumn>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnCollection.TryGet(string)" />
    [Pure]
    public new SqliteColumn? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteColumn>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlColumn, SqliteColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteColumn>();
    }

    /// <inheritdoc />
    protected override SqliteColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new SqliteColumn( Table, ReinterpretCast.To<SqliteColumnBuilder>( builder ) );
    }
}
