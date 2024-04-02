using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Internal;

public static class PostgreSqlHelpers
{
    public const string ByteaMarker = "\\x";
    public const string ByteaTypeCast = "::BYTEA";
    public const string EmptyByteaLiteral = $"'{ByteaMarker}'{ByteaTypeCast}";
    public const string DateFormatQuoted = $"DATE{SqlHelpers.DateFormatQuoted}";
    public const string TimeFormatQuoted = $@"TI\ME{SqlHelpers.TimeFormatMicrosecondQuoted}";
    public const string TimestampFormatQuoted = $@"TI\MESTA\MP{SqlHelpers.DateTimeFormatMicrosecondQuoted}";
    public const string TimestampTzFormatQuoted = $@"TI\MESTA\MPTZ{SqlHelpers.DateTimeFormatMicrosecondQuoted}";
    public const string UpsertExcludedRecordSetName = "EXCLUDED";

    public static readonly SqlSchemaObjectName DefaultVersionHistoryName =
        SqlSchemaObjectName.Create( "public", SqlHelpers.VersionHistoryName );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConnectionStringEntry[] ExtractConnectionStringEntries(NpgsqlConnectionStringBuilder builder)
    {
        var i = 0;
        var result = new SqlConnectionStringEntry[builder.Count];
        foreach ( var e in (DbConnectionStringBuilder)builder )
        {
            var (key, value) = (KeyValuePair<string, object>)e;
            result[i++] = new SqlConnectionStringEntry( key, value, IsMutableConnectionStringKey( key ) );
        }

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string ExtendConnectionString(ReadOnlyArray<SqlConnectionStringEntry> entries, string options)
    {
        var builder = new NpgsqlConnectionStringBuilder( options );
        foreach ( var (key, value, isMutable) in entries )
        {
            if ( ! isMutable || ! builder.ShouldSerialize( key ) )
                builder.Add( key, value );
        }

        return builder.ToString();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsMutableConnectionStringKey(string key)
    {
        return ! key.Equals( "HOST", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "SERVER", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "PORT", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "DATABASE", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "DB", StringComparison.OrdinalIgnoreCase );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1::BOOLEAN" : "0::BOOLEAN";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(Guid value)
    {
        return $"'{value}'";
    }

    [Pure]
    public static string GetDbLiteral(ReadOnlySpan<byte> value)
    {
        if ( value.Length == 0 )
            return EmptyByteaLiteral;

        var length = checked( (value.Length << 1) + ByteaMarker.Length + ByteaTypeCast.Length + 2 );
        var data = length <= SqlHelpers.StackallocThreshold ? stackalloc char[length] : new char[length];
        data[0] = SqlHelpers.TextDelimiter;
        data[1] = ByteaMarker[0];
        data[2] = ByteaMarker[1];
        var index = 3;

        for ( var i = 0; i < value.Length; ++i )
        {
            var b = value[i];
            data[index++] = ToHexChar( b >> 4 );
            data[index++] = ToHexChar( b & 0xF );
        }

        data[index++] = SqlHelpers.TextDelimiter;
        ByteaTypeCast.CopyTo( data.Slice( index ) );
        return new string( data );

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static char ToHexChar(int value)
        {
            Assume.IsInRange( value, 0, 15 );
            return (char)(value < 10 ? '0' + value : 'A' + value - 10);
        }
    }

    public static void AppendCreateDatabase(
        SqlNodeInterpreter interpreter,
        string name,
        string? encodingName,
        string? localeName,
        int? concurrentConnectionsLimit)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "DATABASE" ).AppendSpace();
        interpreter.AppendDelimitedName( name );

        if ( encodingName is not null )
        {
            interpreter.Context.Sql.AppendSpace().Append( "ENCODING" ).AppendSpace().Append( '=' ).AppendSpace();
            interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( encodingName ).Append( SqlHelpers.TextDelimiter );
        }

        if ( localeName is not null )
        {
            interpreter.Context.Sql.AppendSpace().Append( "LOCALE" ).AppendSpace().Append( '=' ).AppendSpace();
            interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( localeName ).Append( SqlHelpers.TextDelimiter );
        }

        if ( concurrentConnectionsLimit is not null )
        {
            interpreter.Context.Sql.AppendSpace().Append( "CONNECTION" ).AppendSpace().Append( "LIMIT" ).AppendSpace();
            interpreter.Context.Sql.Append( '=' ).AppendSpace().Append( concurrentConnectionsLimit.Value );
        }
    }

    public static void AppendCreateSchema(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
    }

    public static void AppendRenameSchema(SqlNodeInterpreter interpreter, string oldName, string newName)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( oldName );
        interpreter.Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( newName );
    }

    public static void AppendDropSchema(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "CASCADE" );
    }

    public static void AppendRenameView(SqlNodeInterpreter interpreter, SqlSchemaObjectName oldName, string newName)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( oldName );
        interpreter.Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( newName );
    }

    public static void AppendAlterTableHeader(SqlNodeInterpreter interpreter, SqlRecordSetInfo info)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        interpreter.AppendDelimitedRecordSetInfo( info );
    }

    public static void AppendAlterTableDropConstraint(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "CONSTRAINT" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendRenameIndex(SqlNodeInterpreter interpreter, SqlSchemaObjectName oldName, string newName)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "INDEX" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( oldName );
        interpreter.Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( newName );
    }

    public static void AppendRenameConstraint(SqlNodeInterpreter interpreter, SqlRecordSetInfo table, string oldName, string newName)
    {
        AppendAlterTableHeader( interpreter, table );
        using ( interpreter.Context.TempIndentIncrease() )
        {
            interpreter.Context.AppendIndent().Append( "RENAME" ).AppendSpace().Append( "CONSTRAINT" ).AppendSpace();
            interpreter.AppendDelimitedName( oldName );
            interpreter.Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
            interpreter.AppendDelimitedName( newName );
        }
    }

    public static void AppendAlterTableAddPrimaryKey(SqlNodeInterpreter interpreter, SqlPrimaryKeyDefinitionNode primaryKey)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
        interpreter.VisitPrimaryKeyDefinition( primaryKey );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableAddCheck(SqlNodeInterpreter interpreter, SqlCheckDefinitionNode check)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
        interpreter.VisitCheckDefinition( check );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableAddForeignKey(SqlNodeInterpreter interpreter, SqlForeignKeyDefinitionNode foreignKey)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
        interpreter.VisitForeignKeyDefinition( foreignKey );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableDropColumn(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableAddColumn(SqlNodeInterpreter interpreter, SqlColumnDefinitionNode column)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.VisitColumnDefinition( column );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableDropColumnExpression(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "EXPRESSION" ).AppendComma();
    }

    public static void AppendAlterTableSetColumnNotNull(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "SET" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" ).AppendComma();
    }

    public static void AppendAlterTableDropColumnNotNull(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" ).AppendComma();
    }

    public static void AppendAlterTableSetColumnDataType(SqlNodeInterpreter interpreter, string name, ISqlDataType dataType)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "SET" ).AppendSpace().Append( "DATA" ).AppendSpace().Append( "TYPE" ).AppendSpace();
        interpreter.Context.Sql.Append( dataType.Name ).AppendComma();
    }

    public static void AppendAlterTableDropColumnDefault(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "DEFAULT" ).AppendComma();
    }

    public static void AppendAlterTableSetColumnDefault(SqlNodeInterpreter interpreter, string name, SqlExpressionNode value)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "SET" ).AppendSpace().Append( "DEFAULT" ).AppendSpace().Append( '(' );

        using ( interpreter.Context.TempChildDepthIncrease() )
        using ( interpreter.Context.TempIndentIncrease() )
            interpreter.Visit( value );

        interpreter.Context.Sql.Append( ')' ).AppendComma();
    }
}
