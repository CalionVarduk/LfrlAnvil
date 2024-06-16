// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

/// <summary>
/// Contains various PostgreSQL helpers.
/// </summary>
public static class PostgreSqlHelpers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public const string ByteaMarker = "\\x";
    public const string ByteaTypeCast = "::BYTEA";
    public const string EmptyByteaLiteral = $"'{ByteaMarker}'{ByteaTypeCast}";
    public const string DateFormatQuoted = $"DATE{SqlHelpers.DateFormatQuoted}";
    public const string TimeFormatQuoted = $@"TI\ME{SqlHelpers.TimeFormatMicrosecondQuoted}";
    public const string TimestampFormatQuoted = $@"TI\MESTA\MP{SqlHelpers.DateTimeFormatMicrosecondQuoted}";
    public const string TimestampTzFormatQuoted = $@"TI\MESTA\MPTZ{SqlHelpers.DateTimeFormatMicrosecondQuoted}";
    public const string UpsertExcludedRecordSetName = "EXCLUDED";
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Default version history table name.
    /// </summary>
    public static readonly SqlSchemaObjectName DefaultVersionHistoryName =
        SqlSchemaObjectName.Create( "public", SqlHelpers.VersionHistoryName );

    /// <summary>
    /// Extracts a collection of <see cref="SqlConnectionStringEntry"/> instances
    /// from the provided <see cref="NpgsqlConnectionStringBuilder"/>.
    /// </summary>
    /// <param name="builder">Source connection string builder.</param>
    /// <returns>New collection of <see cref="SqlConnectionStringEntry"/> instances.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConnectionStringEntry[] ExtractConnectionStringEntries(NpgsqlConnectionStringBuilder builder)
    {
        var i = 0;
        var result = new SqlConnectionStringEntry[builder.Count];
        foreach ( var e in ( DbConnectionStringBuilder )builder )
        {
            var (key, value) = ( KeyValuePair<string, object> )e;
            result[i++] = new SqlConnectionStringEntry( key, value, IsMutableConnectionStringKey( key ) );
        }

        return result;
    }

    /// <summary>
    /// Extends the provided collection of <see cref="SqlConnectionStringEntry"/> instances with a partial connection string,
    /// potentially overriding entries with their <see cref="SqlConnectionStringEntry.IsMutable"/> equal to <b>true</b>.
    /// </summary>
    /// <param name="entries">Connection string entries to extend.</param>
    /// <param name="options">Connection string options to apply to the extended collection.</param>
    /// <returns>New connection string.</returns>
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

    /// <summary>
    /// Specifies whether or not the provided <paramref name="key"/> of a connection string entry is considered to be mutable.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns><b>true</b> when <paramref name="key"/> is mutable, otherwise <b>false</b>.</returns>
    /// <remarks>
    /// Mutable connection string entries can be changed in the
    /// <see cref="ExtendConnectionString(ReadOnlyArray{SqlConnectionStringEntry},string)"/> method invocation.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsMutableConnectionStringKey(string key)
    {
        return ! key.Equals( "HOST", StringComparison.OrdinalIgnoreCase )
            && ! key.Equals( "SERVER", StringComparison.OrdinalIgnoreCase )
            && ! key.Equals( "PORT", StringComparison.OrdinalIgnoreCase )
            && ! key.Equals( "DATABASE", StringComparison.OrdinalIgnoreCase )
            && ! key.Equals( "DB", StringComparison.OrdinalIgnoreCase );
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(bool value)
    {
        return value ? "1::BOOLEAN" : "0::BOOLEAN";
    }

    /// <summary>
    /// Converts <paramref name="value"/> to a DB literal.
    /// </summary>
    /// <param name="value">Value to convert.</param>
    /// <returns><see cref="String"/> representation of the provided <paramref name="value"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetDbLiteral(Guid value)
    {
        return $"'{value}'";
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
            return ( char )(value < 10 ? '0' + value : 'A' + value - 10);
        }
    }

    internal static void AppendCreateDatabase(
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

    internal static void AppendCreateSchema(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
    }

    internal static void AppendRenameSchema(SqlNodeInterpreter interpreter, string oldName, string newName)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( oldName );
        interpreter.Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( newName );
    }

    internal static void AppendDropSchema(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "CASCADE" );
    }

    internal static void AppendRenameView(SqlNodeInterpreter interpreter, SqlSchemaObjectName oldName, string newName)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "VIEW" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( oldName );
        interpreter.Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( newName );
    }

    internal static void AppendAlterTableHeader(SqlNodeInterpreter interpreter, SqlRecordSetInfo info)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        interpreter.AppendDelimitedRecordSetInfo( info );
    }

    internal static void AppendAlterTableDropConstraint(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "CONSTRAINT" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    internal static void AppendRenameIndex(SqlNodeInterpreter interpreter, SqlSchemaObjectName oldName, string newName)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "INDEX" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( oldName );
        interpreter.Context.Sql.AppendSpace().Append( "RENAME" ).AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( newName );
    }

    internal static void AppendRenameConstraint(SqlNodeInterpreter interpreter, SqlRecordSetInfo table, string oldName, string newName)
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

    internal static void AppendAlterTableAddPrimaryKey(SqlNodeInterpreter interpreter, SqlPrimaryKeyDefinitionNode primaryKey)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
        interpreter.VisitPrimaryKeyDefinition( primaryKey );
        interpreter.Context.Sql.AppendComma();
    }

    internal static void AppendAlterTableAddCheck(SqlNodeInterpreter interpreter, SqlCheckDefinitionNode check)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
        interpreter.VisitCheckDefinition( check );
        interpreter.Context.Sql.AppendComma();
    }

    internal static void AppendAlterTableAddForeignKey(SqlNodeInterpreter interpreter, SqlForeignKeyDefinitionNode foreignKey)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace();
        interpreter.VisitForeignKeyDefinition( foreignKey );
        interpreter.Context.Sql.AppendComma();
    }

    internal static void AppendAlterTableDropColumn(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    internal static void AppendAlterTableAddColumn(SqlNodeInterpreter interpreter, SqlColumnDefinitionNode column)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.VisitColumnDefinition( column );
        interpreter.Context.Sql.AppendComma();
    }

    internal static void AppendAlterTableDropColumnExpression(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "EXPRESSION" ).AppendComma();
    }

    internal static void AppendAlterTableSetColumnNotNull(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "SET" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" ).AppendComma();
    }

    internal static void AppendAlterTableDropColumnNotNull(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "NOT" ).AppendSpace().Append( "NULL" ).AppendComma();
    }

    internal static void AppendAlterTableSetColumnDataType(SqlNodeInterpreter interpreter, string name, ISqlDataType dataType)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "SET" ).AppendSpace().Append( "DATA" ).AppendSpace().Append( "TYPE" ).AppendSpace();
        interpreter.Context.Sql.Append( dataType.Name ).AppendComma();
    }

    internal static void AppendAlterTableDropColumnDefault(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "ALTER" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendSpace().Append( "DROP" ).AppendSpace().Append( "DEFAULT" ).AppendComma();
    }

    internal static void AppendAlterTableSetColumnDefault(SqlNodeInterpreter interpreter, string name, SqlExpressionNode value)
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
