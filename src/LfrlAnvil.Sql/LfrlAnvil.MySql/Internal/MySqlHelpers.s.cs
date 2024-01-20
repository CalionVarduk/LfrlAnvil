using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Exceptions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Objects.Builders;
using ExceptionResources = LfrlAnvil.Sql.Exceptions.ExceptionResources;

namespace LfrlAnvil.MySql.Internal;

internal static class MySqlHelpers
{
    public const string GuidFunctionName = "GUID";
    public const string DropIndexIfExistsProcedureName = "_DROP_INDEX_IF_EXISTS";
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

        ExceptionThrower.Throw( new SqlObjectCastException( MySqlDialect.Instance, typeof( T ), obj.GetType() ) );
        return default!;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetFullName(string schemaName, string name)
    {
        return schemaName.Length > 0 ? $"{schemaName}.{name}" : name;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetFullFieldName(string fullTableName, string name)
    {
        return $"{fullTableName}.{name}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDefaultPrimaryKeyName(MySqlTableBuilder table)
    {
        return $"PK_{table.Name}";
    }

    [Pure]
    public static string GetDefaultForeignKeyName(MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
    {
        var builder = new StringBuilder( 32 );
        builder.Append( "FK_" ).Append( originIndex.Table.Name );

        foreach ( var c in originIndex.Columns )
            builder.Append( '_' ).Append( c.Column.Name );

        builder.Append( "_REF_" );
        if ( ! ReferenceEquals( originIndex.Table.Schema, referencedIndex.Table.Schema ) )
            builder.Append( referencedIndex.Table.Schema.Name ).Append( '_' );

        builder.Append( referencedIndex.Table.Name );
        return builder.ToString();
    }

    [Pure]
    public static string GetDefaultCheckName(MySqlTableBuilder table)
    {
        return $"CHK_{table.Name}_{table.Checks.Count}";
    }

    [Pure]
    public static string GetDefaultIndexName(MySqlTableBuilder table, ReadOnlyMemory<MySqlIndexColumnBuilder> columns, bool isUnique)
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
        if ( string.IsNullOrWhiteSpace( name ) || name.Contains( '`' ) || name.Contains( '\'' ) )
            ExceptionThrower.Throw( new MySqlObjectBuilderException( ExceptionResources.InvalidName( name ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1" : "0";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(ulong value)
    {
        return value.ToString( CultureInfo.InvariantCulture );
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
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(float value)
    {
        var result = value.ToString( "G9", CultureInfo.InvariantCulture );
        return IsFloatingPoint( result ) ? result : $"{result}.0";
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

    public static void AppendCreateSchemaStatement(MySqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSemicolon();
    }

    public static void AppendDropSchemaStatement(MySqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSemicolon();
    }

    public static void AppendCreateGuidFunctionStatement(MySqlNodeInterpreter interpreter, string schemaName)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "FUNCTION" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( schemaName, GuidFunctionName );
        interpreter.Context.Sql.Append( '(' ).Append( ')' ).AppendSpace();
        interpreter.Context.Sql.Append( "RETURNS" ).AppendSpace().Append( MySqlDataType.CreateBinary( 16 ).Name );
        interpreter.Context.AppendIndent().Append( "BEGIN" );

        using ( interpreter.Context.TempIndentIncrease() )
        {
            const string vValue = "@value";
            interpreter.Context.AppendIndent().Append( "SET" ).AppendSpace().Append( vValue ).AppendSpace();
            interpreter.Context.Sql.Append( '=' ).AppendSpace().Append( "UNHEX" ).Append( '(' );
            interpreter.Context.Sql.Append( "REPLACE" ).Append( '(' );
            interpreter.Context.Sql.Append( "UUID" ).Append( '(' ).Append( ')' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( TextDelimiter ).Append( '-' ).Append( TextDelimiter ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( EmptyTextLiteral ).Append( ')' ).Append( ')' ).AppendSemicolon();

            interpreter.Context.AppendIndent().Append( "RETURN" ).AppendSpace().Append( "CONCAT" ).Append( '(' );
            interpreter.Context.Sql.Append( "REVERSE" ).Append( '(' ).Append( "SUBSTRING" ).Append( '(' );
            interpreter.Context.Sql.Append( vValue ).AppendComma().AppendSpace().Append( '1' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( '4' ).Append( ')' ).Append( ')' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( "REVERSE" ).Append( '(' ).Append( "SUBSTRING" ).Append( '(' );
            interpreter.Context.Sql.Append( vValue ).AppendComma().AppendSpace().Append( '5' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( '2' ).Append( ')' ).Append( ')' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( "REVERSE" ).Append( '(' ).Append( "SUBSTRING" ).Append( '(' );
            interpreter.Context.Sql.Append( vValue ).AppendComma().AppendSpace().Append( '7' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( '2' ).Append( ')' ).Append( ')' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( "SUBSTRING" ).Append( '(' );
            interpreter.Context.Sql.Append( vValue ).AppendComma().AppendSpace().Append( '9' ).Append( ')' );
            interpreter.Context.Sql.Append( ')' ).AppendSemicolon();
        }

        interpreter.Context.AppendIndent().Append( "END" ).AppendSemicolon();
    }

    public static void AppendDropIndexIfExistsProcedureStatement(MySqlNodeInterpreter interpreter, string schemaName)
    {
        const string pSchemaName = "schema_name";
        const string pTableName = "table_name";
        const string pIndexName = "index_name";

        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "PROCEDURE" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( schemaName, DropIndexIfExistsProcedureName );
        interpreter.Context.Sql.Append( '(' );

        var parameterType = MySqlDataType.CreateVarChar( 128 );
        interpreter.AppendDelimitedName( pSchemaName );
        interpreter.Context.Sql.AppendSpace().Append( parameterType.Name ).AppendComma().AppendSpace();
        interpreter.AppendDelimitedName( pTableName );
        interpreter.Context.Sql.AppendSpace().Append( parameterType.Name ).AppendComma().AppendSpace();
        interpreter.AppendDelimitedName( pIndexName );
        interpreter.Context.Sql.AppendSpace().Append( parameterType.Name ).Append( ')' );
        interpreter.Context.AppendIndent().Append( "BEGIN" );

        using ( interpreter.Context.TempIndentIncrease() )
        {
            const string vSchemaName = "@schema_name";
            interpreter.Context.AppendIndent().Append( "SET" ).AppendSpace().Append( vSchemaName ).AppendSpace();
            interpreter.Context.Sql.Append( '=' ).AppendSpace().Append( "COALESCE" ).Append( '(' );
            interpreter.AppendDelimitedName( pSchemaName );
            interpreter.Context.Sql.AppendComma().AppendSpace().Append( "DATABASE" ).Append( '(' ).Append( ')' );
            interpreter.Context.Sql.Append( ')' ).AppendSemicolon();

            var statistics = SqlRecordSetInfo.Create( "s" );
            interpreter.Context.AppendIndent().Append( "IF" ).AppendSpace().Append( "EXISTS" ).AppendSpace().Append( '(' );
            interpreter.Context.Sql.Append( "SELECT" ).AppendSpace().Append( '*' ).AppendSpace().Append( "FROM" ).AppendSpace();
            interpreter.AppendDelimitedSchemaObjectName( "information_schema", "statistics" );
            interpreter.AppendDelimitedAlias( "s" );
            interpreter.Context.Sql.AppendSpace().Append( "WHERE" ).AppendSpace();
            interpreter.AppendDelimitedDataFieldName( statistics, "table_schema" );
            interpreter.Context.Sql.AppendSpace().Append( '=' ).AppendSpace().Append( vSchemaName );
            interpreter.Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
            interpreter.AppendDelimitedDataFieldName( statistics, "table_name" );
            interpreter.Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
            interpreter.AppendDelimitedName( pTableName );
            interpreter.Context.Sql.AppendSpace().Append( "AND" ).AppendSpace();
            interpreter.AppendDelimitedDataFieldName( statistics, "index_name" );
            interpreter.Context.Sql.AppendSpace().Append( '=' ).AppendSpace();
            interpreter.AppendDelimitedName( pIndexName );
            interpreter.Context.Sql.Append( ')' ).AppendSpace().Append( "THEN" );

            using ( interpreter.Context.TempIndentIncrease() )
            {
                const string vText = "@text";
                interpreter.Context.AppendIndent().Append( "SET" ).AppendSpace().Append( vText ).AppendSpace();
                interpreter.Context.Sql.Append( '=' ).AppendSpace().Append( "CONCAT" ).Append( '(' );
                interpreter.Context.Sql.Append( TextDelimiter ).Append( "DROP" ).AppendSpace().Append( "INDEX" ).AppendSpace();
                interpreter.Context.Sql.Append( interpreter.BeginNameDelimiter ).Append( TextDelimiter ).AppendComma().AppendSpace();
                interpreter.AppendDelimitedName( pIndexName );
                interpreter.Context.Sql.AppendComma().AppendSpace().Append( TextDelimiter );
                interpreter.Context.Sql.Append( interpreter.EndNameDelimiter ).AppendSpace().Append( "ON" ).AppendSpace();
                interpreter.Context.Sql.Append( interpreter.BeginNameDelimiter ).Append( TextDelimiter ).AppendComma().AppendSpace();
                interpreter.Context.Sql.Append( vSchemaName ).AppendComma().AppendSpace().Append( TextDelimiter );
                interpreter.Context.Sql.Append( interpreter.EndNameDelimiter ).AppendDot();
                interpreter.Context.Sql.Append( interpreter.BeginNameDelimiter ).Append( TextDelimiter ).AppendComma().AppendSpace();
                interpreter.AppendDelimitedName( pTableName );
                interpreter.Context.Sql.AppendComma().AppendSpace().Append( TextDelimiter );
                interpreter.Context.Sql.Append( interpreter.EndNameDelimiter ).AppendSemicolon().Append( TextDelimiter );
                interpreter.Context.Sql.Append( ')' ).AppendSemicolon();

                const string vStmt = "stmt";
                interpreter.Context.AppendIndent().Append( "PREPARE" ).AppendSpace().Append( vStmt ).AppendSpace();
                interpreter.Context.Sql.Append( "FROM" ).AppendSpace().Append( vText ).AppendSemicolon();
                interpreter.Context.AppendIndent().Append( "EXECUTE" ).AppendSpace().Append( vStmt ).AppendSemicolon();
            }

            interpreter.Context.AppendIndent().Append( "END" ).AppendSpace().Append( "IF" ).AppendSemicolon();
        }

        interpreter.Context.AppendIndent().Append( "END" ).AppendSemicolon();
    }

    [Pure]
    internal static MySqlIndexColumnBuilder[] CreateIndexColumns(
        MySqlTableBuilder table,
        ReadOnlyMemory<ISqlIndexColumnBuilder> columns,
        bool allowNullableColumns = true)
    {
        if ( columns.Length == 0 )
            throw new MySqlObjectBuilderException( ExceptionResources.IndexMustHaveAtLeastOneColumn );

        var errors = Chain<string>.Empty;
        var uniqueColumnIds = new HashSet<ulong>();

        var span = columns.Span;
        var result = new MySqlIndexColumnBuilder[span.Length];
        for ( var i = 0; i < span.Length; ++i )
        {
            var c = CastOrThrow<MySqlIndexColumnBuilder>( span[i] );
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
            throw new MySqlObjectBuilderException( errors );

        return result;
    }

    internal static void AssertForeignKey(MySqlTableBuilder table, MySqlIndexBuilder originIndex, MySqlIndexBuilder referencedIndex)
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
            throw new MySqlObjectBuilderException( errors );
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
