using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sqlite.Internal;

public static class SqliteHelpers
{
    private const int StackallocThreshold = 64;
    private const char TextDelimiter = '\'';
    private const char BlobMarker = 'X';
    private static readonly string EmptyTextLiteral = $"{TextDelimiter}{TextDelimiter}";
    private static readonly string EmptyBlobLiteral = $"{BlobMarker}{EmptyTextLiteral}";

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
    public static string GetFullFieldName(string fullTableName, string name)
    {
        return $"{fullTableName}.{name}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDefaultPrimaryKeyName(SqliteTableBuilder table)
    {
        return $"PK_{table.Name}";
    }

    [Pure]
    public static string GetDefaultForeignKeyName(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( "FK_" ).Append( originIndex.Table.Name );

        foreach ( var c in originIndex.Columns )
            builder.Append( '_' ).Append( c.Column.Name );

        var refName = ReferenceEquals( originIndex.Table.Schema, referencedIndex.Table.Schema )
            ? referencedIndex.Table.Name
            : referencedIndex.Table.FullName;

        builder.Append( "_REF_" ).Append( refName );
        return builder.ToString();
    }

    [Pure]
    public static string GetDefaultCheckName(SqliteTableBuilder table)
    {
        return $"CHK_{table.Name}_{table.Checks.Count}";
    }

    [Pure]
    public static string GetDefaultIndexName(SqliteTableBuilder table, ReadOnlyMemory<SqliteIndexColumnBuilder> columns, bool isUnique)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( isUnique ? "UIX_" : "IX_" ).Append( table.Name );

        foreach ( var c in columns )
            builder.Append( '_' ).Append( c.Column.Name ).Append( c.Ordering == OrderBy.Asc ? 'A' : 'D' );

        return builder.ToString();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void AssertName(string name)
    {
        if ( string.IsNullOrWhiteSpace( name ) || name.Contains( '"' ) || name.Contains( '\'' ) )
            ExceptionThrower.Throw( new SqliteObjectBuilderException( ExceptionResources.InvalidName( name ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1" : "0";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(long value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(double value)
    {
        var result = value.ToString( "G17", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";

        [Pure]
        static bool IsFloatingPoint(string value)
        {
            foreach ( var c in value )
            {
                if ( c == '.' || char.ToLower( c ) == 'e' )
                    return true;
            }

            return false;
        }
    }

    [Pure]
    public static string GetDbLiteral(string value)
    {
        var delimiterIndex = value.IndexOf( TextDelimiter );
        if ( delimiterIndex == -1 )
            return value.Length == 0 ? EmptyTextLiteral : $"{TextDelimiter}{value}{TextDelimiter}";

        var delimiterCount = GetDelimiterCount( value.AsSpan( delimiterIndex + 1 ) ) + 1;

        var length = checked( value.Length + delimiterCount + 2 );
        var data = length <= StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = TextDelimiter;

        var startIndex = 0;
        var buffer = data.Slice( 1, data.Length - 2 );

        do
        {
            var segment = value.AsSpan( startIndex, delimiterIndex - startIndex );
            segment.CopyTo( buffer );
            buffer[segment.Length] = TextDelimiter;
            buffer[segment.Length + 1] = TextDelimiter;
            buffer = buffer.Slice( segment.Length + 2 );

            startIndex = delimiterIndex + 1;
            delimiterIndex = value.IndexOf( TextDelimiter, startIndex );
        }
        while ( delimiterIndex != -1 );

        value.AsSpan( startIndex ).CopyTo( buffer );
        data[^1] = TextDelimiter;
        return new string( data );

        [Pure]
        static int GetDelimiterCount(ReadOnlySpan<char> text)
        {
            var count = 0;
            for ( var i = 0; i < text.Length; ++i )
            {
                if ( text[i] == TextDelimiter )
                    ++count;
            }

            return count;
        }
    }

    [Pure]
    public static string GetDbLiteral(ReadOnlySpan<byte> value)
    {
        if ( value.Length == 0 )
            return EmptyBlobLiteral;

        var length = checked( (value.Length << 1) + 3 );
        var data = length <= StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = BlobMarker;
        data[1] = TextDelimiter;
        var index = 2;

        for ( var i = 0; i < value.Length; ++i )
        {
            var b = value[i];
            data[index++] = ToHexChar( b >> 4 );
            data[index++] = ToHexChar( b & 0xF );
        }

        data[^1] = TextDelimiter;
        return new string( data );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static char ToHexChar(int value)
        {
            Assume.IsInRange( value, 0, 15 );
            return (char)(value < 10 ? '0' + value : 'A' + value - 10);
        }
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

    internal static void AssertForeignKey(SqliteTableBuilder table, SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex)
    {
        var errors = Chain<string>.Empty;

        if ( ReferenceEquals( originIndex, referencedIndex ) )
            errors = errors.Extend( ExceptionResources.ForeignKeyOriginIndexAndReferencedIndexAreTheSame );

        if ( ! ReferenceEquals( table, originIndex.Table ) )
            errors = errors.Extend( ExceptionResources.ObjectDoesNotBelongToTable( originIndex, table ) );

        if ( ! ReferenceEquals( originIndex.Database, referencedIndex.Database ) )
            errors = errors.Extend( ExceptionResources.ObjectBelongsToAnotherDatabase( referencedIndex ) );

        if ( originIndex.IsRemoved )
            errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( originIndex ) );

        if ( referencedIndex.IsRemoved )
            errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( referencedIndex ) );

        if ( ! referencedIndex.IsUnique )
            errors = errors.Extend( ExceptionResources.IndexIsNotMarkedAsUnique( referencedIndex ) );

        var indexColumns = originIndex.Columns.Span;
        var referencedIndexColumns = referencedIndex.Columns.Span;

        foreach ( var c in referencedIndexColumns )
        {
            if ( c.Column.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( c.Column ) );
        }

        if ( indexColumns.Length != referencedIndexColumns.Length )
            errors = errors.Extend( ExceptionResources.ForeignKeyOriginIndexAndReferencedIndexMustHaveTheSameAmountOfColumns );
        else
        {
            for ( var i = 0; i < indexColumns.Length; ++i )
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
