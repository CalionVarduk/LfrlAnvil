using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Extensions;

public static class PostgreSqlObjectExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlIndexBuilder SetFilter(this PostgreSqlIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
    {
        return index.SetFilter( filter( index.Table.Node ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlPrimaryKeyBuilder SetPrimaryKey(
        this PostgreSqlConstraintBuilderCollection constraints,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlPrimaryKeyBuilder SetPrimaryKey(
        this PostgreSqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlIndexBuilder CreateIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlIndexBuilder CreateIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlIndexBuilder CreateUniqueIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlIndexBuilder CreateUniqueIndex(
        this PostgreSqlConstraintBuilderCollection constraints,
        string name,
        params SqlOrderByNode[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlColumnBuilder SetType(this PostgreSqlColumnBuilder column, PostgreSqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlColumnBuilder SetType(this PostgreSqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlColumnBuilder SetType<T>(this PostgreSqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlColumnBuilder SetDefaultValue<T>(this PostgreSqlColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static PostgreSqlColumnBuilder SetDefaultValue<T>(this PostgreSqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    public static ISqlDatabaseBuilder ForPostgreSql(this ISqlDatabaseBuilder builder, Action<PostgreSqlDatabaseBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlSchemaBuilder ForPostgreSql(this ISqlSchemaBuilder builder, Action<PostgreSqlSchemaBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlTableBuilder ForPostgreSql(this ISqlTableBuilder builder, Action<PostgreSqlTableBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlColumnBuilder ForPostgreSql(this ISqlColumnBuilder builder, Action<PostgreSqlColumnBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlIndexBuilder ForPostgreSql(this ISqlIndexBuilder builder, Action<PostgreSqlIndexBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlForeignKeyBuilder ForPostgreSql(this ISqlForeignKeyBuilder builder, Action<PostgreSqlForeignKeyBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlPrimaryKeyBuilder ForPostgreSql(this ISqlPrimaryKeyBuilder builder, Action<PostgreSqlPrimaryKeyBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

    public static ISqlCheckBuilder ForPostgreSql(this ISqlCheckBuilder builder, Action<PostgreSqlCheckBuilder> action)
    {
        return ForPostgreSqlImpl( builder, action );
    }

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
