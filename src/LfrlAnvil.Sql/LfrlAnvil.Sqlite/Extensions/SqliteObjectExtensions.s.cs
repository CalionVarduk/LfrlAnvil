using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Extensions;

public static class SqliteObjectExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder Get(this SqliteIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Get( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder Create(this SqliteIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Create( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder GetOrCreate(this SqliteIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.GetOrCreate( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndexBuilder SetFilter(this SqliteIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
    {
        return index.SetFilter( filter( index.Table.ToRecordSet() ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlitePrimaryKeyBuilder SetPrimaryKey(this SqliteTableBuilder table, params ISqlIndexColumnBuilder[] columns)
    {
        return table.SetPrimaryKey( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetType(this SqliteColumnBuilder column, SqliteDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetDefaultForDataType( dataType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetType(this SqliteColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetType<T>(this SqliteColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetDefaultValue<T>(this SqliteColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteColumnBuilder SetDefaultValue<T>(this SqliteColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqliteIndex Get(this SqliteIndexCollection indexes, params ISqlIndexColumn[] columns)
    {
        return indexes.Get( columns );
    }

    public static ISqlDatabaseBuilder ForSqlite(this ISqlDatabaseBuilder builder, Action<SqliteDatabaseBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    public static ISqlSchemaBuilder ForSqlite(this ISqlSchemaBuilder builder, Action<SqliteSchemaBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    public static ISqlTableBuilder ForSqlite(this ISqlTableBuilder builder, Action<SqliteTableBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    public static ISqlColumnBuilder ForSqlite(this ISqlColumnBuilder builder, Action<SqliteColumnBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    public static ISqlIndexBuilder ForSqlite(this ISqlIndexBuilder builder, Action<SqliteIndexBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    public static ISqlForeignKeyBuilder ForSqlite(this ISqlForeignKeyBuilder builder, Action<SqliteForeignKeyBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

    public static ISqlPrimaryKeyBuilder ForSqlite(this ISqlPrimaryKeyBuilder builder, Action<SqlitePrimaryKeyBuilder> action)
    {
        return ForSqliteImpl( builder, action );
    }

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
