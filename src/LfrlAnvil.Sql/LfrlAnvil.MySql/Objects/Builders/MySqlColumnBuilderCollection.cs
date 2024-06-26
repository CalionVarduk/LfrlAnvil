﻿// Copyright 2024 Łukasz Furlepa
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
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlColumnBuilderCollection : SqlColumnBuilderCollection
{
    internal MySqlColumnBuilderCollection(MySqlColumnTypeDefinitionProvider typeDefinitions)
        : base( typeDefinitions.GetByType<object>() ) { }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Table" />
    public new MySqlTableBuilder Table => ReinterpretCast.To<MySqlTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlColumnBuilderCollection.SetDefaultTypeDefinition(SqlColumnTypeDefinition)" />
    public new MySqlColumnBuilderCollection SetDefaultTypeDefinition(SqlColumnTypeDefinition definition)
    {
        base.SetDefaultTypeDefinition( definition );
        return this;
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Get(string)" />
    [Pure]
    public new MySqlColumnBuilder Get(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.Get( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.TryGet(string)" />
    [Pure]
    public new MySqlColumnBuilder? TryGet(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.TryGet( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.Create(string)" />
    public new MySqlColumnBuilder Create(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.Create( name ) );
    }

    /// <inheritdoc cref="SqlColumnBuilderCollection.GetOrCreate(string)" />
    public new MySqlColumnBuilder GetOrCreate(string name)
    {
        return ReinterpretCast.To<MySqlColumnBuilder>( base.GetOrCreate( name ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{TSource,TDestination}"/> instance.</returns>
    [Pure]
    public new SqlObjectBuilderEnumerator<SqlColumnBuilder, MySqlColumnBuilder> GetEnumerator()
    {
        return base.GetEnumerator().UnsafeReinterpretAs<MySqlColumnBuilder>();
    }

    /// <inheritdoc />
    protected override MySqlColumnBuilder CreateColumnBuilder(string name)
    {
        return new MySqlColumnBuilder( Table, name, DefaultTypeDefinition );
    }
}
