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
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlColumnCollection : SqlColumnCollection
{
    internal MySqlColumnCollection(MySqlColumnBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlColumnCollection.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlColumnCollection.Get(string)" />
    [Pure]
    public new MySqlColumn Get(string name)
    {
        return ReinterpretCast.To<MySqlColumn>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnCollection.TryGet(string)" />
    [Pure]
    public new MySqlColumn? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlColumn>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlColumn, MySqlColumn> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlColumn>();
    }

    /// <inheritdoc />
    protected override MySqlColumn CreateColumn(SqlColumnBuilder builder)
    {
        return new MySqlColumn( Table, ReinterpretCast.To<MySqlColumnBuilder>( builder ) );
    }
}
