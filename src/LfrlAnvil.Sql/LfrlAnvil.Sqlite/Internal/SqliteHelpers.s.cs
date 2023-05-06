using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Builders;
using LfrlAnvil.Sqlite.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sqlite.Internal;

public static class SqliteHelpers
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T CastOrThrow<T>(object obj)
    {
        if ( obj is T t )
            return t;

        ExceptionThrower.Throw( new SqliteObjectCastException( typeof( T ), obj.GetType() ) );
        return default!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetFullName(string schemaName, string name)
    {
        return schemaName.Length > 0 ? $"{schemaName}_{name}" : name;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDefaultPrimaryKeyName(SqliteTableBuilder table)
    {
        return $"PK_{table.Name}";
    }

    [Pure]
    public static string GetDefaultForeignKeyName(SqliteIndexBuilder index, SqliteIndexBuilder referencedIndex)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( "FK_" ).Append( index.Table.Name );

        foreach ( var c in index.Columns )
            builder.Append( '_' ).Append( c.Column.Name );

        builder.Append( "_REF_" ).Append( referencedIndex.Table.FullName );
        return builder.ToString();
    }

    [Pure]
    public static string GetDefaultIndexName(SqliteTableBuilder table, ReadOnlyMemory<SqliteIndexColumnBuilder> columns, bool isUnique)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( isUnique ? "UIX_" : "IX_" ).Append( table.Name );

        foreach ( var c in columns.Span )
            builder.Append( '_' ).Append( c.Column.Name ).Append( c.Ordering == OrderBy.Asc ? 'A' : 'D' );

        return builder.ToString();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void AssertName(string name)
    {
        if ( string.IsNullOrWhiteSpace( name ) || name.Contains( '"' ) )
            throw new SqliteObjectBuilderException( ExceptionResources.InvalidName( name ) );
    }

    [Pure]
    internal static SqliteIndexColumnBuilder[] CreateIndexColumns(
        SqliteTableBuilder table,
        ReadOnlyMemory<ISqlIndexColumnBuilder> columns,
        bool allowNullableColumns = true)
    {
        if ( columns.Length == 0 )
            throw new SqliteObjectBuilderException( ExceptionResources.IndexMustHaveAtLeastOneColumn );

        var errors = Chain<string>.Empty;
        var uniqueColumnIds = new HashSet<ulong>();

        var span = columns.Span;
        var result = new SqliteIndexColumnBuilder[span.Length];
        for ( var i = 0; i < span.Length; ++i )
        {
            var c = CastOrThrow<SqliteIndexColumnBuilder>( span[i] );
            result[i] = c;

            if ( ! uniqueColumnIds.Add( c.Column.Id ) )
            {
                errors = errors.Extend( ExceptionResources.ColumnIsDuplicated( c.Column ) );
                continue;
            }

            if ( ! ReferenceEquals( c.Column.Table, table ) )
                errors = errors.Extend( ExceptionResources.ObjectDoesNotBelongToTable( c.Column, table ) );

            if ( c.Column.IsRemoved )
                errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( c.Column ) );

            if ( ! allowNullableColumns && c.Column.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( c.Column ) );
        }

        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );

        return result;
    }

    internal static void AssertForeignKey(SqliteTableBuilder table, SqliteIndexBuilder index, SqliteIndexBuilder referencedIndex)
    {
        var errors = Chain<string>.Empty;

        if ( ReferenceEquals( index, referencedIndex ) )
            errors = errors.Extend( ExceptionResources.ForeignKeyIndexAndReferencedIndexAreTheSame );

        if ( ! ReferenceEquals( table, index.Table ) )
            errors = errors.Extend( ExceptionResources.ObjectDoesNotBelongToTable( index, table ) );

        if ( ! ReferenceEquals( index.Database, referencedIndex.Database ) )
            errors = errors.Extend( ExceptionResources.ObjectBelongsToAnotherDatabase( referencedIndex ) );

        if ( index.IsRemoved )
            errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( index ) );

        if ( referencedIndex.IsRemoved )
            errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( referencedIndex ) );

        if ( ! referencedIndex.IsUnique )
            errors = errors.Extend( ExceptionResources.IndexIsNotMarkedAsUnique( referencedIndex ) );

        var indexColumns = index.Columns;
        var referencedIndexColumns = referencedIndex.Columns;

        for ( var i = 0; i < referencedIndexColumns.Count; ++i )
        {
            var column = referencedIndexColumns[i].Column;
            if ( column.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( column ) );
        }

        if ( indexColumns.Count != referencedIndexColumns.Count )
            errors = errors.Extend( ExceptionResources.ForeignKeyIndexAndReferencedIndexMustHaveTheSameAmountOfColumns );
        else
        {
            for ( var i = 0; i < indexColumns.Count; ++i )
            {
                var column = indexColumns[i].Column;
                var refColumn = referencedIndexColumns[i].Column;
                if ( column.TypeDefinition.RuntimeType != refColumn.TypeDefinition.RuntimeType )
                    errors = errors.Extend( ExceptionResources.ColumnTypesAreIncompatible( column, refColumn ) );
            }
        }

        if ( errors.Count > 0 )
            throw new SqliteObjectBuilderException( errors );
    }
}
