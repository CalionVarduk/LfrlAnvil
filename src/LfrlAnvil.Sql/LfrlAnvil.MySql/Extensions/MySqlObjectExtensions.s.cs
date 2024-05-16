﻿using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Extensions;

/// <summary>
/// Contains various <see cref="SqlObjectBuilder"/> and <see cref="SqlObject"/> extension methods.
/// </summary>
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public static class MySqlObjectExtensions
{
    /// <summary>
    /// Changes <see cref="SqlIndexBuilder.Filter"/> value of the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Source index.</param>
    /// <param name="filter">Value to set.</param>
    /// <returns><paramref name="index"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When filter cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlIndexBuilder SetFilter(this MySqlIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
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
    public static MySqlPrimaryKeyBuilder SetPrimaryKey(this MySqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
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
    public static MySqlPrimaryKeyBuilder SetPrimaryKey(
        this MySqlConstraintBuilderCollection constraints,
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
    public static MySqlIndexBuilder CreateIndex(this MySqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
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
    public static MySqlIndexBuilder CreateIndex(
        this MySqlConstraintBuilderCollection constraints,
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
    public static MySqlIndexBuilder CreateUniqueIndex(this MySqlConstraintBuilderCollection constraints, params SqlOrderByNode[] columns)
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
    public static MySqlIndexBuilder CreateUniqueIndex(
        this MySqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="dataType"><see cref="MySqlDataType"/> to use for retrieving a default type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlColumnBuilder SetType(this MySqlColumnBuilder column, MySqlDataType dataType)
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
    public static MySqlColumnBuilder SetType(this MySqlColumnBuilder column, Type type)
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
    public static MySqlColumnBuilder SetType<T>(this MySqlColumnBuilder column)
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
    public static MySqlColumnBuilder SetDefaultValue<T>(this MySqlColumnBuilder column, T? value)
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
    public static MySqlColumnBuilder SetDefaultValue<T>(this MySqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlDatabaseBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlDatabaseBuilder ForMySql(this ISqlDatabaseBuilder builder, Action<MySqlDatabaseBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlSchemaBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlSchemaBuilder ForMySql(this ISqlSchemaBuilder builder, Action<MySqlSchemaBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlTableBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlTableBuilder ForMySql(this ISqlTableBuilder builder, Action<MySqlTableBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlColumnBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlColumnBuilder ForMySql(this ISqlColumnBuilder builder, Action<MySqlColumnBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlIndexBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlIndexBuilder ForMySql(this ISqlIndexBuilder builder, Action<MySqlIndexBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlForeignKeyBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlForeignKeyBuilder ForMySql(this ISqlForeignKeyBuilder builder, Action<MySqlForeignKeyBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlPrimaryKeyBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlPrimaryKeyBuilder ForMySql(this ISqlPrimaryKeyBuilder builder, Action<MySqlPrimaryKeyBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlCheckBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlCheckBuilder ForMySql(this ISqlCheckBuilder builder, Action<MySqlCheckBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="MySqlViewBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlViewBuilder ForMySql(this ISqlViewBuilder builder, Action<MySqlViewBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T ForMySqlImpl<T, TImpl>(T builder, Action<TImpl> action)
        where T : class
        where TImpl : T
    {
        if ( builder is TImpl mySql )
            action( mySql );

        return builder;
    }
}
