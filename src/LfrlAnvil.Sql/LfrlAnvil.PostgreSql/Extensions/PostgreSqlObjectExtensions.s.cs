using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Extensions;

/// <summary>
/// Contains various <see cref="SqlObjectBuilder"/> and <see cref="SqlObject"/> extension methods.
/// </summary>
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public static class PostgreSqlObjectExtensions
{
    /// <summary>
    /// Changes <see cref="SqlIndexBuilder.Filter"/> value of the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Source index.</param>
    /// <param name="filter">Value to set.</param>
    /// <returns><paramref name="index"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When filter cannot be changed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlIndexBuilder SetFilter(this PostgreSqlIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
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
    public static PostgreSqlPrimaryKeyBuilder SetPrimaryKey(
        this PostgreSqlConstraintBuilderCollection constraints,
        params SqlOrderByNode[] columns)
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
    public static PostgreSqlPrimaryKeyBuilder SetPrimaryKey(
        this PostgreSqlConstraintBuilderCollection constraints,
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
    public static PostgreSqlIndexBuilder CreateIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        params SqlOrderByNode[] columns)
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
    public static PostgreSqlIndexBuilder CreateIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
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
    public static PostgreSqlIndexBuilder CreateUniqueIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        params SqlOrderByNode[] columns)
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
    public static PostgreSqlIndexBuilder CreateUniqueIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    /// <summary>
    /// Changes <see cref="SqlColumnBuilder.TypeDefinition"/> value of the provided <paramref name="column"/>.
    /// </summary>
    /// <param name="column">Source column.</param>
    /// <param name="dataType"><see cref="PostgreSqlDataType"/> to use for retrieving a default type definition associated with it.</param>
    /// <returns><paramref name="column"/>.</returns>
    /// <exception cref="SqlObjectBuilderException">When type definition cannot be changed.</exception>
    /// <remarks>Changing the type will reset the <see cref="SqlColumnBuilder.DefaultValue"/> to null.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlColumnBuilder SetType(this PostgreSqlColumnBuilder column, PostgreSqlDataType dataType)
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
    public static PostgreSqlColumnBuilder SetType(this PostgreSqlColumnBuilder column, Type type)
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
    public static PostgreSqlColumnBuilder SetType<T>(this PostgreSqlColumnBuilder column)
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
    public static PostgreSqlColumnBuilder SetDefaultValue<T>(this PostgreSqlColumnBuilder column, T? value)
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
    public static PostgreSqlColumnBuilder SetDefaultValue<T>(this PostgreSqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlDatabaseBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlDatabaseBuilder ForPostgreSql(this ISqlDatabaseBuilder builder, Action<PostgreSqlDatabaseBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlSchemaBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlSchemaBuilder ForPostgreSql(this ISqlSchemaBuilder builder, Action<PostgreSqlSchemaBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlTableBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlTableBuilder ForPostgreSql(this ISqlTableBuilder builder, Action<PostgreSqlTableBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlColumnBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlColumnBuilder ForPostgreSql(this ISqlColumnBuilder builder, Action<PostgreSqlColumnBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlIndexBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlIndexBuilder ForPostgreSql(this ISqlIndexBuilder builder, Action<PostgreSqlIndexBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlForeignKeyBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlForeignKeyBuilder ForPostgreSql(this ISqlForeignKeyBuilder builder, Action<PostgreSqlForeignKeyBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlPrimaryKeyBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlPrimaryKeyBuilder ForPostgreSql(this ISqlPrimaryKeyBuilder builder, Action<PostgreSqlPrimaryKeyBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlCheckBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlCheckBuilder ForPostgreSql(this ISqlCheckBuilder builder, Action<PostgreSqlCheckBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    /// <summary>
    /// Invokes the provided <paramref name="action"/> only when the <paramref name="builder"/>
    /// is an instance of <see cref="PostgreSqlViewBuilder"/> type.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="action">Action to invoke.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ISqlViewBuilder ForPostgreSql(this ISqlViewBuilder builder, Action<PostgreSqlViewBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static T ForPostgreSqlImpl<T, TImpl>(T builder, Action<TImpl> action)
        where T : class
        where TImpl : T
    {
        if ( builder is TImpl postgreSql )
            action( postgreSql );

        return builder;
    }
}
