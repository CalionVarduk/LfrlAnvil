using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using MySqlConnector;

namespace LfrlAnvil.MySql.Internal;

internal static class MySqlHelpers
{
    public static readonly SqlSchemaObjectName DefaultVersionHistoryName = SqlSchemaObjectName.Create( "common", "__VersionHistory" );
    public const string GuidFunctionName = "GUID";
    public const string DropIndexIfExistsProcedureName = "_DROP_INDEX_IF_EXISTS";
    public const string DateFormatQuoted = $"DATE{SqlHelpers.DateFormatQuoted}";
    public const string TimeFormatQuoted = $@"TI\ME{SqlHelpers.TimeFormatMicrosecondQuoted}";
    public const string DefaultUpdateSourceAlias = "new";
    public const int DefaultIndexPrefixLength = 500;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlConnectionStringEntry[] ExtractConnectionStringEntries(MySqlConnectionStringBuilder builder)
    {
        var i = 0;
        var result = new SqlConnectionStringEntry[builder.Count];
        foreach ( var e in builder )
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
        var builder = new MySqlConnectionStringBuilder( options );
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
        return ! key.Equals( "Server", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Host", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Data Source", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "DataSource", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Address", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Addr", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Network Address", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Port", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "Allow User Variables", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "AllowUserVariables", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "GUID Format", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "GuidFormat", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "No Backslash Escapes", StringComparison.OrdinalIgnoreCase ) &&
            ! key.Equals( "NoBackslashEscapes", StringComparison.OrdinalIgnoreCase );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string GetUpdateSourceAlias(MySqlNodeInterpreterOptions options)
    {
        return string.IsNullOrEmpty( options.UpsertSourceAlias ) ? DefaultUpdateSourceAlias : options.UpsertSourceAlias;
    }

    public static void AppendAlterTableHeader(SqlNodeInterpreter interpreter, SqlRecordSetInfo info)
    {
        interpreter.Context.Sql.Append( "ALTER" ).AppendSpace().Append( "TABLE" ).AppendSpace();
        interpreter.AppendDelimitedRecordSetInfo( info );
    }

    public static void AppendAlterTableDropForeignKey(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "FOREIGN" ).AppendSpace().Append( "KEY" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableDropPrimaryKey(SqlNodeInterpreter interpreter)
    {
        interpreter.Context.AppendIndent();
        interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "PRIMARY" ).AppendSpace().Append( "KEY" ).AppendComma();
    }

    public static void AppendAlterTableDropCheck(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "CHECK" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableRenameIndex(SqlNodeInterpreter interpreter, string originalName, string name)
    {
        interpreter.Context.AppendIndent();
        interpreter.Context.Sql.Append( "RENAME" ).AppendSpace().Append( "INDEX" ).AppendSpace();
        interpreter.AppendDelimitedName( originalName );
        interpreter.Context.Sql.AppendSpace().Append( "TO" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableDropColumn(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.AppendIndent().Append( "DROP" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableChangeColumn(SqlNodeInterpreter interpreter, string originalName, SqlColumnDefinitionNode column)
    {
        interpreter.Context.AppendIndent().Append( "CHANGE" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.AppendDelimitedName( originalName );
        interpreter.Context.Sql.AppendSpace();
        interpreter.VisitColumnDefinition( column );
        interpreter.Context.Sql.AppendComma();
    }

    public static void AppendAlterTableAddColumn(SqlNodeInterpreter interpreter, SqlColumnDefinitionNode column)
    {
        interpreter.Context.AppendIndent().Append( "ADD" ).AppendSpace().Append( "COLUMN" ).AppendSpace();
        interpreter.VisitColumnDefinition( column );
        interpreter.Context.Sql.AppendComma();
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

    public static void AppendCreateSchema(
        SqlNodeInterpreter interpreter,
        string name,
        string? characterSetName,
        string? collationName,
        bool? isEncrypted)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );

        if ( characterSetName is not null )
        {
            interpreter.Context.Sql.AppendSpace().Append( "CHARACTER" ).AppendSpace().Append( "SET" ).AppendSpace();
            interpreter.Context.Sql.Append( '=' ).AppendSpace();
            interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( characterSetName ).Append( SqlHelpers.TextDelimiter );
        }

        if ( collationName is not null )
        {
            interpreter.Context.Sql.AppendSpace().Append( "COLLATE" ).AppendSpace().Append( '=' ).AppendSpace();
            interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( collationName ).Append( SqlHelpers.TextDelimiter );
        }

        if ( isEncrypted is not null )
        {
            var symbol = isEncrypted.Value ? 'Y' : 'N';
            interpreter.Context.Sql.AppendSpace().Append( "ENCRYPTION" ).AppendSpace().Append( '=' ).AppendSpace();
            interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( symbol ).Append( SqlHelpers.TextDelimiter );
        }
    }

    public static void AppendDropSchema(SqlNodeInterpreter interpreter, string name)
    {
        interpreter.Context.Sql.Append( "DROP" ).AppendSpace().Append( "SCHEMA" ).AppendSpace();
        interpreter.AppendDelimitedName( name );
    }

    public static void AppendCreateGuidFunction(SqlNodeInterpreter interpreter, string schemaName)
    {
        interpreter.Context.Sql.Append( "CREATE" ).AppendSpace().Append( "FUNCTION" ).AppendSpace();
        interpreter.AppendDelimitedSchemaObjectName( schemaName, GuidFunctionName );
        interpreter.Context.Sql.Append( '(' ).Append( ')' ).AppendSpace();
        interpreter.Context.Sql.Append( "RETURNS" ).AppendSpace().Append( MySqlDataTypeProvider.Guid.Name );
        interpreter.Context.AppendIndent().Append( "BEGIN" );

        using ( interpreter.Context.TempIndentIncrease() )
        {
            const string vValue = "@value";
            interpreter.Context.AppendIndent().Append( "SET" ).AppendSpace().Append( vValue ).AppendSpace();
            interpreter.Context.Sql.Append( '=' ).AppendSpace().Append( "UNHEX" ).Append( '(' );
            interpreter.Context.Sql.Append( "REPLACE" ).Append( '(' );
            interpreter.Context.Sql.Append( "UUID" ).Append( '(' ).Append( ')' ).AppendComma().AppendSpace();
            interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( '-' ).Append( SqlHelpers.TextDelimiter );
            interpreter.Context.Sql.AppendComma().AppendSpace();

            interpreter.Context.Sql.Append( SqlHelpers.EmptyTextLiteral ).Append( ')' ).Append( ')' ).AppendSemicolon();

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

        interpreter.Context.AppendIndent().Append( "END" );
    }

    public static void AppendCreateDropIndexIfExistsProcedure(SqlNodeInterpreter interpreter, string schemaName)
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
                interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).Append( "DROP" ).AppendSpace().Append( "INDEX" ).AppendSpace();
                interpreter.Context.Sql.Append( interpreter.BeginNameDelimiter );
                interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).AppendComma().AppendSpace();

                interpreter.AppendDelimitedName( pIndexName );
                interpreter.Context.Sql.AppendComma().AppendSpace().Append( SqlHelpers.TextDelimiter );
                interpreter.Context.Sql.Append( interpreter.EndNameDelimiter ).AppendSpace().Append( "ON" ).AppendSpace();
                interpreter.Context.Sql.Append( interpreter.BeginNameDelimiter );
                interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).AppendComma().AppendSpace();

                interpreter.Context.Sql.Append( vSchemaName ).AppendComma().AppendSpace().Append( SqlHelpers.TextDelimiter );
                interpreter.Context.Sql.Append( interpreter.EndNameDelimiter ).AppendDot().Append( interpreter.BeginNameDelimiter );
                interpreter.Context.Sql.Append( SqlHelpers.TextDelimiter ).AppendComma().AppendSpace();

                interpreter.AppendDelimitedName( pTableName );
                interpreter.Context.Sql.AppendComma().AppendSpace().Append( SqlHelpers.TextDelimiter );
                interpreter.Context.Sql.Append( interpreter.EndNameDelimiter ).AppendSemicolon().Append( SqlHelpers.TextDelimiter );
                interpreter.Context.Sql.Append( ')' ).AppendSemicolon();

                const string vStmt = "stmt";
                interpreter.Context.AppendIndent().Append( "PREPARE" ).AppendSpace().Append( vStmt ).AppendSpace();
                interpreter.Context.Sql.Append( "FROM" ).AppendSpace().Append( vText ).AppendSemicolon();
                interpreter.Context.AppendIndent().Append( "EXECUTE" ).AppendSpace().Append( vStmt ).AppendSemicolon();
            }

            interpreter.Context.AppendIndent().Append( "END" ).AppendSpace().Append( "IF" ).AppendSemicolon();
        }

        interpreter.Context.AppendIndent().Append( "END" );
    }
}
