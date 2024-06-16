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
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal SqliteColumnBuilderCollection(SqliteColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByDataType( SqliteDataType.Any ) ) { }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlColumnBuilderCollection.SetDefaultTypeDefinition(SqlColumnTypeDefinition)" />
    public new SqliteColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Get(string)" />
    [Pure]
    public new SqliteColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.TryGet(string)" />
    [Pure]
    public new SqliteColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Create(string)" />
    public new SqliteColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.GetOrCreate(string)" />
    public new SqliteColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<SqliteColumnBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, SqliteColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<SqliteColumnBuilder>();
    }

    /// <inheritdoc />
    protected override SqliteColumnBuilder CreateColumnBuilder(string name)
    {
        return new SqliteColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
