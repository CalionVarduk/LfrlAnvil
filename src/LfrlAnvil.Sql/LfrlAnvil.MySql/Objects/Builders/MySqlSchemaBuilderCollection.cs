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
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlSchemaBuilderCollection : SqlSchemaBuilderCollection
{
    internal MySqlSchemaBuilderCollection() { }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Database" />
    public new MySqlDatabaseBuilder Database => ReinterpretCast.To<MySqlDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Default" />
    public new MySqlSchemaBuilder Default => ReinterpretCast.To<MySqlSchemaBuilder>( base.Default );

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Get(string)" />
    [Pure]
    public new MySqlSchemaBuilder Get(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.TryGet(string)" />
    [Pure]
    public new MySqlSchemaBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.Create(string)" />
    public new MySqlSchemaBuilder Create(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlSchemaBuilderCollection.GetOrCreate(string)" />
    public new MySqlSchemaBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<MySqlSchemaBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlSchemaBuilder, MySqlSchemaBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlSchemaBuilder>();
    }

    /// <inheritdoc />
    protected override MySqlSchemaBuilder CreateSchemaBuilder(string name)
    {
        return new MySqlSchemaBuilder( Database, name );
    }
}
