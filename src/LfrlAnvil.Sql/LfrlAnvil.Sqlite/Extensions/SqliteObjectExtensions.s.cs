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

using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Extensions;

/// <summary>
/// Contains various <see cref="SqlObjectBuilder"/> and <see cref="SqlObject"/> extension methods.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public static class SqliteObjectExtensions
{
    /// <summary>
    /// Changes <see cref="SqlIndexBuilder.Filter"/> value of the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Source index.</param>
    /// <param name="filter">Value to set.</param>
    /// <returns><paramref name="index"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When filter cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder SetFilter(this SqliteIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
    {
        return index.SetFilter( filter( index.Table.Node ) );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name and sets a new primary key builder with a default name based on that index.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the underlying index.</param>
    /// <returns>New <see cref="SqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When unique index constraint or primary key constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlitePrimaryKeyBuilder SetPrimaryKey(this SqliteConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name and sets a new primary key builder based on that index.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the primary key constraint.</param>
    /// <param name="columns">Collection of columns that define the underlying index.</param>
    /// <returns>New <see cref="SqlPrimaryKeyBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When unique index constraint or primary key constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlitePrimaryKeyBuilder SetPrimaryKey(
        this SqliteConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    /// <summary>
    /// Creates a new index builder with a default name.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder CreateIndex(this SqliteConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    /// <summary>
    /// Creates a new index builder.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder CreateIndex(
        this SqliteConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    /// <summary>
    /// Creates a new unique index builder with a default name.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder CreateUniqueIndex(this SqliteConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    /// <summary>
    /// Creates a new unique index builder.
    /// </summary>
    /// <param name="constraints">Source collection.</param>
    /// <param name="name">Name of the index constraint.</param>
    /// <param name="columns">Collection of columns that define the index.</param>
    /// <returns>New <see cref="SqlIndexBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When index constraint could not be created.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder CreateUniqueIndex(
        this SqliteConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="dataType"><see cref="SqliteDataType"/> to use for retrieving a default type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetType(this SqliteColumnBuilder column, SqliteDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="type">Runtime type to use for retrieving a type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetType(this SqliteColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <typeparam name="T">Runtime type to use for retrieving a type definition associated with it.</typeparam>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetType<T>(this SqliteColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.DefaultValue"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="value">Value to set.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetDefaultValue<T>(this SqliteColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Changes <see cref="ISqlColumnBuilder.DefaultValue"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="value">Value to set.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When default value cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetDefaultValue<T>(this SqliteColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteDatabaseBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlDatabaseBuilder ForSqlite(this ISqlDatabaseBuilder builder, Action<SqliteDatabaseBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteSchemaBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlSchemaBuilder ForSqlite(this ISqlSchemaBuilder builder, Action<SqliteSchemaBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteTableBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlTableBuilder ForSqlite(this ISqlTableBuilder builder, Action<SqliteTableBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteColumnBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlColumnBuilder ForSqlite(this ISqlColumnBuilder builder, Action<SqliteColumnBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteIndexBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlIndexBuilder ForSqlite(this ISqlIndexBuilder builder, Action<SqliteIndexBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteForeignKeyBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlForeignKeyBuilder ForSqlite(this ISqlForeignKeyBuilder builder, Action<SqliteForeignKeyBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqlitePrimaryKeyBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlPrimaryKeyBuilder ForSqlite(this ISqlPrimaryKeyBuilder builder, Action<SqlitePrimaryKeyBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteCheckBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlCheckBuilder ForSqlite(this ISqlCheckBuilder builder, Action<SqliteCheckBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="SqliteViewBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlViewBuilder ForSqlite(this ISqlViewBuilder builder, Action<SqliteViewBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T ForSqliteImpl<T, TImpl>(T builder, Action<TImpl> action)
        where T : class
        where TImpl : T
    {
        if ( builder is TImpl sqlite )
            action( sqlite );

        return builder;
    }
}
