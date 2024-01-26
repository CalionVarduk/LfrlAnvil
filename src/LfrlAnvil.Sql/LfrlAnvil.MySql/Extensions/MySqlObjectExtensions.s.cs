using System;
using System.Runtime.CompilerServices;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Extensions;

public static class MySqlObjectExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlIndexBuilder SetFilter(this MySqlIndexBuilder index, Func<SqlTableBuilderNode, SqlConditionNode?> filter)
    {
        return index.SetFilter( filter( index.Table.RecordSet ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlPrimaryKeyBuilder SetPrimaryKey(
        this MySqlConstraintBuilderCollection constraints,
        params MySqlIndexColumnBuilder[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlPrimaryKeyBuilder SetPrimaryKey(
        this MySqlConstraintBuilderCollection constraints,
        string name,
        params MySqlIndexColumnBuilder[] columns)
    {
        var index = constraints.CreateUniqueIndex( columns );
        return constraints.SetPrimaryKey( name, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlIndexBuilder CreateIndex(
        this MySqlConstraintBuilderCollection constraints,
        params MySqlIndexColumnBuilder[] columns)
    {
        return constraints.CreateIndex( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlIndexBuilder CreateIndex(
        this MySqlConstraintBuilderCollection constraints,
        string name,
        params MySqlIndexColumnBuilder[] columns)
    {
        return constraints.CreateIndex( name, columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlIndexBuilder CreateUniqueIndex(
        this MySqlConstraintBuilderCollection constraints,
        params MySqlIndexColumnBuilder[] columns)
    {
        return constraints.CreateIndex( columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlIndexBuilder CreateUniqueIndex(
        this MySqlConstraintBuilderCollection constraints,
        string name,
        params MySqlIndexColumnBuilder[] columns)
    {
        return constraints.CreateIndex( name, columns, isUnique: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlColumnBuilder SetType(this MySqlColumnBuilder column, MySqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByDataType( dataType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlColumnBuilder SetType(this MySqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlColumnBuilder SetType<T>(this MySqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlColumnBuilder SetDefaultValue<T>(this MySqlColumnBuilder column, T? value)
        where T : notnull
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MySqlColumnBuilder SetDefaultValue<T>(this MySqlColumnBuilder column, T? value)
        where T : struct
    {
        return column.SetDefaultValue( SqlNode.Literal( value ) );
    }

    public static ISqlDatabaseBuilder ForMySql(this ISqlDatabaseBuilder builder, Action<MySqlDatabaseBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlSchemaBuilder ForMySql(this ISqlSchemaBuilder builder, Action<MySqlSchemaBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlTableBuilder ForMySql(this ISqlTableBuilder builder, Action<MySqlTableBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlColumnBuilder ForMySql(this ISqlColumnBuilder builder, Action<MySqlColumnBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlIndexBuilder ForMySql(this ISqlIndexBuilder builder, Action<MySqlIndexBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlForeignKeyBuilder ForMySql(this ISqlForeignKeyBuilder builder, Action<MySqlForeignKeyBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlPrimaryKeyBuilder ForMySql(this ISqlPrimaryKeyBuilder builder, Action<MySqlPrimaryKeyBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

    public static ISqlCheckBuilder ForMySql(this ISqlCheckBuilder builder, Action<MySqlCheckBuilder> action)
    {
        return ForMySqlImpl( builder, action );
    }

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
