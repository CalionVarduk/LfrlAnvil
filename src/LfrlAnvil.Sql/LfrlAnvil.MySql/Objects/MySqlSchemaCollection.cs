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
public sealed class MySqlSchemaCollection : SqlSchemaCollection
{
    internal MySqlSchemaCollection(MySqlSchemaBuilderCollection source)
        : base( source ) { }

    /// <inheritdoc cref="SqlSchemaCollection.Default" />
    public new MySqlSchema Default => ReinterpretCast.To<MySqlSchema>( base.Default );

    /// <inheritdoc cref="SqlSchemaCollection.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );

    /// <inheritdoc cref="SqlSchemaCollection.Get(string)" />
    [Pure]
    public new MySqlSchema Get(string name)
    {
        return ReinterpretCast.To<MySqlSchema>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaCollection.TryGet(string)" />
    [Pure]
    public new MySqlSchema? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlSchema>( base.TryGet( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectEnumerator<SqlSchema, MySqlSchema> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlSchema>();
    }

    /// <inheritdoc />
    protected override MySqlSchema CreateSchema(SqlSchemaBuilder builder)
    {
        return new MySqlSchema( Database, ReinterpretCast.To<MySqlSchemaBuilder>( builder ) );
    }
}
