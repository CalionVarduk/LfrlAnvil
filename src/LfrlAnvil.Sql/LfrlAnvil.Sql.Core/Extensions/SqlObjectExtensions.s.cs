using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlObjectExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Contains(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Contains( columns );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder Get(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Get( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder Create(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Create( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndexBuilder GetOrCreate(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.GetOrCreate( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Remove(this ISqlIndexBuilderCollection indexes, params ISqlIndexColumnBuilder[] columns)
    {
        return indexes.Remove( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlPrimaryKeyBuilder SetPrimaryKey(this ISqlTableBuilder table, params ISqlIndexColumnBuilder[] columns)
    {
        return table.SetPrimaryKey( columns );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType(this ISqlColumnBuilder column, ISqlDataType dataType)
    {
        return column.SetType( column.Database.TypeDefinitions.GetDefaultForDataType( dataType ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType(this ISqlColumnBuilder column, Type type)
    {
        return column.SetType( column.Database.TypeDefinitions.GetByType( type ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlColumnBuilder SetType<T>(this ISqlColumnBuilder column)
    {
        return column.SetType( typeof( T ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsSelfReference(this ISqlForeignKeyBuilder foreignKey)
    {
        return ReferenceEquals( foreignKey.Index.Table, foreignKey.ReferencedIndex.Table );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Contains(this ISqlIndexCollection indexes, params ISqlIndexColumn[] columns)
    {
        return indexes.Contains( columns );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlIndex Get(this ISqlIndexCollection indexes, params ISqlIndexColumn[] columns)
    {
        return indexes.Get( columns );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsSelfReference(this ISqlForeignKey foreignKey)
    {
        return ReferenceEquals( foreignKey.Index.Table, foreignKey.ReferencedIndex.Table );
    }
}
