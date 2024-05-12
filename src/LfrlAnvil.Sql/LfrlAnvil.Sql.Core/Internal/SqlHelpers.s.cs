using System;
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

/// <summary>
/// Contains various SQL helpers.
/// </summary>
public static class SqlHelpers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string VersionHistoryName = "__VersionHistory";
    public const string VersionHistoryOrdinalName = "Ordinal";
    public const string VersionHistoryVersionMajorName = "VersionMajor";
    public const string VersionHistoryVersionMinorName = "VersionMinor";
    public const string VersionHistoryVersionBuildName = "VersionBuild";
    public const string VersionHistoryVersionRevisionName = "VersionRevision";
    public const string VersionHistoryDescriptionName = "Description";
    public const string VersionHistoryCommitDateUtcName = "CommitDateUtc";
    public const string VersionHistoryCommitDurationInTicksName = "CommitDurationInTicks";
    public const string DateFormat = "yyyy-MM-dd";
    public const string TimeFormatMicrosecond = "HH:mm:ss.ffffff";
    public const string DateTimeFormatMicrosecond = $"{DateFormat} {TimeFormatMicrosecond}";
    public const string TimeFormatTick = $"{TimeFormatMicrosecond}f";
    public const string DateTimeFormatTick = $"{DateFormat} {TimeFormatTick}";
    public const string DateTimeOffsetFormat = $"{DateTimeFormatTick}zzz";
    public const string DateFormatQuoted = $@"\'{DateFormat}\'";
    public const string TimeFormatMicrosecondQuoted = $@"\'{TimeFormatMicrosecond}\'";
    public const string DateTimeFormatMicrosecondQuoted = $@"\'{DateTimeFormatMicrosecond}\'";
    public const string TimeFormatTickQuoted = $@"\'{TimeFormatTick}\'";
    public const string DateTimeFormatTickQuoted = $@"\'{DateTimeFormatTick}\'";
    public const string DateTimeOffsetFormatQuoted = $@"\'{DateTimeFormatTick}zzz\'";
    public const string DecimalFormat = "0.0###########################";
    public const string DecimalFormatQuoted = $@"\'{DecimalFormat}\'";
    public const string EmptyTextLiteral = "\'\'";
    public const string EmptyBlobLiteral = $"X{EmptyTextLiteral}";
    public const char TextDelimiter = '\'';
    public const char BlobMarker = 'X';
    public const int StackallocThreshold = 64;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// SQL object name comparer. Equivalent to <see cref="StringComparer.OrdinalIgnoreCase"/>.
    /// </summary>
    public static readonly StringComparer NameComparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Non-query <see cref="IDbCommand"/> executor delegate.
    /// </summary>
    public static readonly Func<IDbCommand, int> ExecuteNonQueryDelegate = static cmd => cmd.ExecuteNonQuery();

    /// <summary>
    /// Scalar <see cref="IDbCommand"/> executor delegate that returns <see cref="Boolean"/> value.
    /// </summary>
    public static readonly Func<IDbCommand, bool> ExecuteBoolScalarDelegate = static cmd => Convert.ToBoolean( cmd.ExecuteScalar() );

    /// <summary>
    /// Default creator of <see cref="ISqlDefaultObjectNameProvider"/> instances.
    /// </summary>
    public static readonly SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =
        static (_, _) => new SqlDefaultObjectNameProvider();

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><b>"1"</b> when <paramref name="value"/> is equal to <b>true</b>, otherwise <b>"0"</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1" : "0";
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(long value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(ulong value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(double value)
    {
        var result = value.ToString( "G17", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(float value)
    {
        var result = value.ToString( "G9", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(decimal value)
    {
        return value.ToString( DecimalFormat, CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(char value)
    {
        return $"{TextDelimiter}{value}{TextDelimiter}";
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    /// <remarks>Escapes <see cref="TextDelimiter"/> occurrences.</remarks>
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

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
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
            return ( char )(value < 10 ? '0' + value : 'A' + value - 10);
        }
    }

    /// <summary>
    /// Returns the full name of an SQL object.
    /// </summary>
    /// <param name="schemaName">SQL schema name.</param>
    /// <param name="name">SQL object name.</param>
    /// <param name="separator">Name separator. Equal to <b>.</b> by default.</param>
    /// <returns>Full SQL object name.</returns>
    [Pure]
    public static string GetFullName(string schemaName, string name, char separator = '.')
    {
        return schemaName.Length > 0 ? $"{schemaName}{separator}{name}" : name;
    }

    /// <summary>
    /// Returns the full name of an SQL field.
    /// </summary>
    /// <param name="schemaName">SQL schema name.</param>
    /// <param name="recordSetName">SQL record set name.</param>
    /// <param name="name">SQL field name.</param>
    /// <param name="firstSeparator">
    /// <paramref name="schemaName"/> and <paramref name="recordSetName"/> separator. Equal to <b>.</b> by default.
    /// </param>
    /// <param name="secondSeparator">
    /// <paramref name="recordSetName"/> and <paramref name="name"/> separator. Equal to <b>.</b> by default.
    /// </param>
    /// <returns>Full SQL field name.</returns>
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

    /// <summary>
    /// Creates a default primary key constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the primary key belongs to.</param>
    /// <returns>Default primary key constraint name.</returns>
    [Pure]
    public static string GetDefaultPrimaryKeyName(ISqlTableBuilder table)
    {
        return $"PK_{table.Name}";
    }

    /// <summary>
    /// Creates a default foreign key constraint name.
    /// </summary>
    /// <param name="originIndex"><see cref="ISqlIndexBuilder"/> from which the foreign key originates.</param>
    /// <param name="referencedIndex"><see cref="ISqlIndexBuilder"/> which the foreign key references.</param>
    /// <returns>Default foreign key constraint name.</returns>
    [Pure]
    public static string GetDefaultForeignKeyName(ISqlIndexBuilder originIndex, ISqlIndexBuilder referencedIndex)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( "FK_" ).Append( originIndex.Table.Name );

        var nextExpressionNo = 1;
        foreach ( var column in originIndex.Columns )
        {
            builder.Append( '_' );
            if ( column is not null )
                builder.Append( column.Name );
            else
                builder.Append( 'E' ).Append( nextExpressionNo++ );
        }

        builder
            .Append( "_REF_" )
            .Append(
                ReferenceEquals( originIndex.Table.Schema, referencedIndex.Table.Schema )
                    ? referencedIndex.Table.Name
                    : GetFullName( referencedIndex.Table.Schema.Name, referencedIndex.Table.Name, separator: '_' ) );

        return builder.ToString();
    }

    /// <summary>
    /// Creates a default check constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the check belongs to.</param>
    /// <returns>Default check constraint name.</returns>
    [Pure]
    public static string GetDefaultCheckName(ISqlTableBuilder table)
    {
        return $"CHK_{table.Name}_{Guid.NewGuid():N}";
    }

    /// <summary>
    /// Creates a default index constraint name.
    /// </summary>
    /// <param name="table"><see cref="ISqlTableBuilder"/> that the index belongs to.</param>
    /// <param name="columns">Collection of columns that belong to the index.</param>
    /// <param name="isUnique">Specifies whether or not the index is unique.</param>
    /// <returns>Default index constraint name.</returns>
    [Pure]
    public static string GetDefaultIndexName(ISqlTableBuilder table, SqlIndexBuilderColumns<ISqlColumnBuilder> columns, bool isUnique)
    {
        var builder = new StringBuilder( 32 );
        if ( isUnique )
            builder.Append( 'U' );

        builder.Append( "IX_" ).Append( table.Name );

        var nextExpressionNo = 1;
        for ( var i = 0; i < columns.Expressions.Count; ++i )
        {
            var node = columns.Expressions[i];
            var column = columns.TryGet( i );

            builder.Append( '_' );
            if ( column is not null )
                builder.Append( column.Name );
            else
                builder.Append( 'E' ).Append( nextExpressionNo++ );

            builder.Append( node.Ordering == OrderBy.Asc ? 'A' : 'D' );
        }

        return builder.ToString();
    }

    /// <summary>
    /// Validates index constraint columns.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> that the index belongs to.</param>
    /// <param name="columns">Collection of columns that belong to the index.</param>
    /// <param name="isUnique">Specifies whether or not the index is unique.</param>
    /// <exception cref="SqlObjectBuilderException">When index columns are not considered valid.</exception>
    /// <remarks>
    /// Index must contain at least one column and columns must be distinct. When index is unique, then it cannot contain expressions.
    /// </remarks>
    public static void AssertIndexColumns(SqlTableBuilder table, SqlIndexBuilderColumns<SqlColumnBuilder> columns, bool isUnique)
    {
        if ( columns.Expressions.Count == 0 )
            throw CreateObjectBuilderException( table.Database, ExceptionResources.IndexMustHaveAtLeastOneColumn );

        var errors = Chain<string>.Empty;
        var uniqueColumnIds = new HashSet<ulong>();
        var expressionFound = false;

        for ( var i = 0; i < columns.Expressions.Count; ++i )
        {
            var column = columns.TryGet( i );
            if ( column is null )
            {
                if ( isUnique && ! expressionFound )
                {
                    expressionFound = true;
                    errors = errors.Extend( ExceptionResources.UniqueIndexCannotContainExpressions );
                }

                continue;
            }

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

    /// <summary>
    /// Validates primary key constraint.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> that the primary key belongs to.</param>
    /// <param name="index"><see cref="SqlIndexBuilder"/> that is the underlying index of the primary key.</param>
    /// <exception cref="SqlObjectBuilderException">When primary key is not considered valid.</exception>
    /// <remarks>
    /// Underlying index must be unique and cannot be partial. It also cannot contain nullable columns or columns that are generated.
    /// </remarks>
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

        var expressionsFound = false;
        foreach ( var c in index.Columns )
        {
            if ( c is null )
            {
                if ( ! expressionsFound )
                {
                    expressionsFound = true;
                    errors = errors.Extend( ExceptionResources.IndexContainsExpressions( index ) );
                }

                continue;
            }

            if ( c.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( c ) );

            if ( c.Computation is not null )
                errors = errors.Extend( ExceptionResources.ColumnIsGenerated( c ) );
        }

        if ( errors.Count > 0 )
            throw CreateObjectBuilderException( table.Database, errors );
    }

    /// <summary>
    /// Validates foreign key constraint.
    /// </summary>
    /// <param name="table"><see cref="SqlTableBuilder"/> that the foreign key belongs to.</param>
    /// <param name="originIndex"><see cref="SqlIndexBuilder"/> from which the foreign key originates.</param>
    /// <param name="referencedIndex"><see cref="SqlIndexBuilder"/> which the foreign key references.</param>
    /// <exception cref="SqlObjectBuilderException">When foreign key is not considered valid.</exception>
    /// <remarks>
    /// Indexes must not be the same.
    /// Origin index cannot contain expressions.
    /// Referenced index must be unique and cannot be partial, and cannot contain nullable or generated columns.
    /// Both origin and referenced index must contain the same number of columns and their runtime types must be sequentially equal.
    /// </remarks>
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
        foreach ( var c in indexColumns )
        {
            if ( c is null )
            {
                errors = errors.Extend( ExceptionResources.IndexContainsExpressions( originIndex ) );
                break;
            }
        }

        var expressionsFound = false;
        var referencedIndexColumns = referencedIndex.Columns;
        foreach ( var c in referencedIndexColumns )
        {
            if ( c is null )
            {
                if ( ! expressionsFound )
                {
                    expressionsFound = true;
                    errors = errors.Extend( ExceptionResources.IndexContainsExpressions( referencedIndex ) );
                }

                continue;
            }

            if ( c.IsNullable )
                errors = errors.Extend( ExceptionResources.ColumnIsNullable( c ) );

            if ( c.Computation is not null )
                errors = errors.Extend( ExceptionResources.ColumnIsGenerated( c ) );
        }

        if ( indexColumns.Expressions.Count != referencedIndexColumns.Expressions.Count )
            errors = errors.Extend( ExceptionResources.ForeignKeyOriginIndexAndReferencedIndexMustHaveTheSameAmountOfColumns );
        else
        {
            for ( var i = 0; i < indexColumns.Expressions.Count; ++i )
            {
                var column = indexColumns.TryGet( i );
                var refColumn = referencedIndexColumns.TryGet( i );
                if ( column is null || refColumn is null )
                    continue;

                if ( column.TypeDefinition.RuntimeType != refColumn.TypeDefinition.RuntimeType )
                    errors = errors.Extend( ExceptionResources.ColumnTypesAreIncompatible( column, refColumn ) );
            }
        }

        if ( errors.Count > 0 )
            throw CreateObjectBuilderException( table.Database, errors );
    }

    /// <summary>
    /// Type casts the provided object to the desired type.
    /// </summary>
    /// <param name="database">SQL database builder with which the object is associated.</param>
    /// <param name="obj">Object to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns><paramref name="obj"/> cast to the desired type.</returns>
    /// <exception cref="SqlObjectCastException">When <paramref name="obj"/> is not of the desired type.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T CastOrThrow<T>(ISqlDatabaseBuilder database, object obj)
    {
        return CastOrThrow<T>( database.Dialect, obj );
    }

    /// <summary>
    /// Type casts the provided object to the desired type.
    /// </summary>
    /// <param name="dialect">SQL dialect with which the object is associated.</param>
    /// <param name="obj">Object to cast.</param>
    /// <typeparam name="T">Desired type.</typeparam>
    /// <returns><paramref name="obj"/> cast to the desired type.</returns>
    /// <exception cref="SqlObjectCastException">When <paramref name="obj"/> is not of the desired type.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T CastOrThrow<T>(SqlDialect dialect, object obj)
    {
        if ( obj is T result )
            return result;

        ExceptionThrower.Throw( new SqlObjectCastException( dialect, expected: typeof( T ), actual: obj.GetType() ) );
        return default!;
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderException"/> instance.
    /// </summary>
    /// <param name="database">Source SQL database builder.</param>
    /// <param name="error">Error message.</param>
    /// <returns>New <see cref="SqlObjectBuilderException"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderException CreateObjectBuilderException(ISqlDatabaseBuilder database, string error)
    {
        return CreateObjectBuilderException( database, Chain.Create( error ) );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderException"/> instance.
    /// </summary>
    /// <param name="database">Source SQL database builder.</param>
    /// <param name="errors">Collection of error messages.</param>
    /// <returns>New <see cref="SqlObjectBuilderException"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderException CreateObjectBuilderException(ISqlDatabaseBuilder database, Chain<string> errors)
    {
        return new SqlObjectBuilderException( database.Dialect, errors );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectCastException"/> instance.
    /// </summary>
    /// <param name="database">Source SQL database builder.</param>
    /// <param name="expected">Expected object type.</param>
    /// <param name="actual">Actual object type.</param>
    /// <returns>New <see cref="SqlObjectCastException"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectCastException CreateObjectCastException(ISqlDatabaseBuilder database, Type expected, Type actual)
    {
        return new SqlObjectCastException( database.Dialect, expected, actual );
    }

    /// <summary>
    /// Creates a new <see cref="SqlObjectCastException"/> instance.
    /// </summary>
    /// <param name="database">Source SQL database.</param>
    /// <param name="expected">Expected object type.</param>
    /// <param name="actual">Actual object type.</param>
    /// <returns>New <see cref="SqlObjectCastException"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectCastException CreateObjectCastException(ISqlDatabase database, Type expected, Type actual)
    {
        return new SqlObjectCastException( database.Dialect, expected, actual );
    }

    /// <summary>
    /// Extracts <see cref="SqlObjectBuilder.ReferencingObjects"/> from the provided <paramref name="obj"/>, on order of their creation.
    /// </summary>
    /// <param name="obj"><see cref="SqlObjectBuilder"/> instance to extract references from.</param>
    /// <param name="filter">
    /// Optional SQL object builder reference filter. References that return <b>false</b> will be ignored. Equal to null by default.
    /// </param>
    /// <returns>New <see cref="RentedMemorySequence{T}"/> instance.</returns>
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
