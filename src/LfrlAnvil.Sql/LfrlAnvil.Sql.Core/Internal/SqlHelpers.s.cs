﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Memory;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.Sql.Internal;

public static class SqlHelpers
{
    public const string VersionHistoryName = "__VersionHistory";
    public const string VersionHistoryOrdinalName = "Ordinal";
    public const string VersionHistoryVersionMajorName = "VersionMajor";
    public const string VersionHistoryVersionMinorName = "VersionMinor";
    public const string VersionHistoryVersionBuildName = "VersionBuild";
    public const string VersionHistoryVersionRevisionName = "VersionRevision";
    public const string VersionHistoryDescriptionName = "Description";
    public const string VersionHistoryCommitDateUtcName = "CommitDateUtc";
    public const string VersionHistoryCommitDurationInTicksName = "CommitDurationInTicks";
    public const char TextDelimiter = '\'';
    public const char BlobMarker = 'X';
    public const int StackallocThreshold = 64;
    public static readonly string EmptyTextLiteral = $"{TextDelimiter}{TextDelimiter}";
    public static readonly string EmptyBlobLiteral = $"{BlobMarker}{EmptyTextLiteral}";
    public static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;
    public static readonly Func<IDbCommand, int> ExecuteNonQueryDelegate = static cmd => cmd.ExecuteNonQuery();

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
    public static string GetDbLiteral(ulong value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(double value)
    {
        var result = value.ToString( "G17", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(float value)
    {
        var result = value.ToString( "G9", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(decimal value)
    {
        return value.ToString( "0.0###########################", CultureInfo.InvariantCulture );
    }

    [Pure]
    public static string GetDbLiteral(ReadOnlySpan<char> value)
    {
        var delimiterIndex = value.IndexOf( TextDelimiter );
        if ( delimiterIndex == -1 )
            return value.Length == 0 ? EmptyTextLiteral : $"{TextDelimiter}{value}{TextDelimiter}";

        var delimiterCount = GetDelimiterCount( value.Slice( delimiterIndex + 1 ) ) + 1;

        var length = checked( value.Length + delimiterCount + 2 );
        var data = length <= StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = TextDelimiter;

        var buffer = data.Slice( 1, data.Length - 2 );

        do
        {
            var segment = value.Slice( 0, delimiterIndex );
            segment.CopyTo( buffer );
            buffer[segment.Length] = TextDelimiter;
            buffer[segment.Length + 1] = TextDelimiter;
            buffer = buffer.Slice( segment.Length + 2 );
            value = value.Slice( delimiterIndex + 1 );
            delimiterIndex = value.IndexOf( TextDelimiter );
        }
        while ( delimiterIndex != -1 );

        value.CopyTo( buffer );
        data[^1] = TextDelimiter;
        return new string( data );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static int GetDelimiterCount(ReadOnlySpan<char> text)
        {
            var count = 0;
            for ( var i = text.IndexOf( TextDelimiter ); i != -1; i = text.IndexOf( TextDelimiter ) )
            {
                text = text.Slice( i + 1 );
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
    public static string GetFullName(string schemaName, string name, char separator = '.')
    {
        return schemaName.Length > 0 ? $"{schemaName}{separator}{name}" : name;
    }

    [Pure]
    public static string GetFullName(
        string schemaName,
        string recordSetName,
        string name,
        char firstSeparator = '.',
        char secondSeparator = '.')
    {
        return schemaName.Length > 0
            ? $"{schemaName}{firstSeparator}{recordSetName}{secondSeparator}{name}"
            : $"{recordSetName}{secondSeparator}{name}";
    }

    [Pure]
    public static string GetDefaultPrimaryKeyName(ISqlTableBuilder table)
    {
        return $"PK_{table.Name}";
    }

    [Pure]
    public static string GetDefaultForeignKeyName(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( "FK_" ).Append( originIndex.Table.Name );

        foreach ( var c in originIndex.Columns )
            builder.Append( '_' ).Append( c.Column.Name );

        builder
            .Append( "_REF_" )
            .Append(
                ReferenceEquals( originIndex.Table.Schema, referencedIndex.Table.Schema )
                    ? referencedIndex.Table.Name
                    : GetFullName( referencedIndex.Table.Schema.Name, referencedIndex.Table.Name, separator: '_' ) );

        return builder.ToString();
    }

    [Pure]
    public static string GetDefaultCheckName(ISqlTableBuilder table)
    {
        return $"CHK_{table.Name}_{Guid.NewGuid():N}";
    }

    [Pure]
    public static string GetDefaultIndexName<T>(ISqlTableBuilder table, ReadOnlyArray<SqlIndexColumnBuilder<T>> columns, bool isUnique)
        where T : class, ISqlColumnBuilder
    {
        var builder = new StringBuilder( 32 );
        if ( isUnique )
            builder.Append( 'U' );

        builder.Append( "IX_" ).Append( table.Name );

        foreach ( var c in columns )
            builder.Append( '_' ).Append( c.Column.Name ).Append( c.Ordering == OrderBy.Asc ? 'A' : 'D' );

        return builder.ToString();
    }

    public static void AssertIndexColumns(SqlTableBuilder table, ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> columns)
    {
        if ( columns.Count == 0 )
            throw CreateObjectBuilderException( table.Database, ExceptionResources.IndexMustHaveAtLeastOneColumn );

        var errors = Chain<string>.Empty;
        var uniqueColumnIds = new HashSet<ulong>();

        for ( var i = 0; i < columns.Count; ++i )
        {
            var column = CastOrThrow<SqlColumnBuilder>( table.Database, columns[i].Column );

            if ( ! uniqueColumnIds.Add( column.Id ) )
            {
                errors = errors.Extend( ExceptionResources.ColumnIsDuplicated( column ) );
                continue;
            }

            if ( ! ReferenceEquals( column.Table, table ) )
                errors = errors.Extend( ExceptionResources.ObjectDoesNotBelongToTable( column, table ) );

            if ( column.IsRemoved )
                errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( column ) );
        }

        if ( errors.Count > 0 )
            throw CreateObjectBuilderException( table.Database, errors );
    }

    public static void AssertPrimaryKey(SqlTableBuilder table, SqlIndexBuilder index)
    {
        var errors = Chain<string>.Empty;
        if ( index.IsRemoved )
            errors = errors.Extend( ExceptionResources.ObjectHasBeenRemoved( index ) );

        if ( ! index.IsUnique )
            errors = errors.Extend( ExceptionResources.IndexIsNotMarkedAsUnique( index ) );

        if ( index.Filter is not null )
            errors = errors.Extend( ExceptionResources.IndexIsPartial( index ) );

        if ( ! ReferenceEquals( index.Table, table ) )
            errors = errors.Extend( ExceptionResources.ObjectDoesNotBelongToTable( index, table ) );

        foreach ( var c in index.Columns )
        {
            if ( c.Column.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( c.Column ) );
        }

        if ( errors.Count > 0 )
            throw CreateObjectBuilderException( table.Database, errors );
    }

    public static void AssertForeignKey(SqlTableBuilder table, SqlIndexBuilder originIndex, SqlIndexBuilder referencedIndex)
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

        if ( referencedIndex.Filter is not null )
            errors = errors.Extend( ExceptionResources.IndexIsPartial( referencedIndex ) );

        var indexColumns = originIndex.Columns;
        var referencedIndexColumns = referencedIndex.Columns;

        foreach ( var c in referencedIndexColumns )
        {
            if ( c.Column.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( c.Column ) );
        }

        if ( indexColumns.Count != referencedIndexColumns.Count )
            errors = errors.Extend( ExceptionResources.ForeignKeyOriginIndexAndReferencedIndexMustHaveTheSameAmountOfColumns );
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
            throw CreateObjectBuilderException( table.Database, errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T CastOrThrow<T>(ISqlDatabaseBuilder database, object obj)
    {
        return CastOrThrow<T>( database.Dialect, obj );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T CastOrThrow<T>(SqlDialect dialect, object obj)
    {
        if ( obj is T result )
            return result;

        ExceptionThrower.Throw( new SqlObjectCastException( dialect, expected: typeof( T ), actual: obj.GetType() ) );
        return default!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderException CreateObjectBuilderException(ISqlDatabaseBuilder database, string error)
    {
        return CreateObjectBuilderException( database, Chain.Create( error ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderException CreateObjectBuilderException(ISqlDatabaseBuilder database, Chain<string> errors)
    {
        return new SqlObjectBuilderException( database.Dialect, errors );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectCastException CreateObjectCastException(ISqlDatabaseBuilder database, Type expected, Type actual)
    {
        return new SqlObjectCastException( database.Dialect, expected, actual );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectCastException CreateObjectCastException(ISqlDatabase database, Type expected, Type actual)
    {
        return new SqlObjectCastException( database.Dialect, expected, actual );
    }

    public static RentedMemorySequence<SqlObjectBuilder> GetReferencingObjectsInOrderOfCreation(
        SqlObjectBuilder obj,
        Func<SqlObjectBuilderReference<SqlObjectBuilder>, bool>? filter = null)
    {
        if ( obj.ReferencedTargets is null || obj.ReferencedTargets.Count == 0 )
            return RentedMemorySequence<SqlObjectBuilder>.Empty;

        var result = obj.Database.ObjectPool.GreedyRent();
        try
        {
            foreach ( var reference in obj.ReferencingObjects )
            {
                if ( filter is null || filter( reference ) )
                    result.Push( reference.Source.Object );
            }
        }
        catch
        {
            result.Dispose();
            throw;
        }

        if ( result.Length == 0 )
        {
            result.Dispose();
            return RentedMemorySequence<SqlObjectBuilder>.Empty;
        }

        result.Sort( static (a, b) => a.Id.CompareTo( b.Id ) );
        return result;
    }

    [Pure]
    private static bool IsFloatingPoint(string value)
    {
        foreach ( var c in value )
        {
            if ( c == '.' || char.ToLower( c ) == 'e' )
                return true;
        }

        return false;
    }
}
