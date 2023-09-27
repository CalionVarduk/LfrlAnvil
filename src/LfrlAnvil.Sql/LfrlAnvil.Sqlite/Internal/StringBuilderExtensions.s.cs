using System;
using System.Runtime.CompilerServices;
using System.Text;
using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Internal;

internal static class StringBuilderExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder RemoveLastNewLine(this StringBuilder builder)
    {
        Assume.IsGreaterThanOrEqualTo( builder.Length, Environment.NewLine.Length, nameof( builder.Length ) );
        builder.Length -= Environment.NewLine.Length;
        return builder;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendName(this StringBuilder builder, string name)
    {
        const char delimiter = '"';
        return builder.Append( delimiter ).Append( name ).Append( delimiter );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendIndentation(this StringBuilder builder, int level = 1)
    {
        const int size = 2;
        Assume.IsGreaterThan( level, 0, nameof( level ) );
        return builder.Append( ' ', repeatCount: level * size );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendCommandEnd(this StringBuilder builder)
    {
        return builder.Append( ';' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendTokenSeparator(this StringBuilder builder)
    {
        return builder.Append( ' ' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendElementSeparator(this StringBuilder builder)
    {
        return builder.Append( ',' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendElementsBegin(this StringBuilder builder)
    {
        return builder.Append( '(' );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendElementsEnd(this StringBuilder builder, int trimCount = 0)
    {
        Assume.IsGreaterThanOrEqualTo( trimCount, 0, nameof( trimCount ) );
        builder.Length -= trimCount;
        return builder.Append( ')' );
    }

    internal static StringBuilder AppendColumnDefinition(
        this StringBuilder builder,
        string name,
        SqliteColumnTypeDefinition valueType,
        bool isNullable,
        object? defaultValue)
    {
        builder
            .AppendName( name )
            .AppendTokenSeparator()
            .Append( valueType.DbType.Name );

        if ( ! isNullable )
            builder.AppendTokenSeparator().Append( "NOT NULL" );

        if ( defaultValue is not null )
        {
            builder
                .AppendTokenSeparator()
                .Append( "DEFAULT" )
                .AppendTokenSeparator()
                .AppendElementsBegin()
                .Append( valueType.TryToDbLiteral( defaultValue ) )
                .AppendElementsEnd();
        }

        return builder;
    }

    internal static StringBuilder AppendDropColumn(this StringBuilder builder, string fullTableName, string columnName)
    {
        return builder
            .Append( "ALTER TABLE" )
            .AppendTokenSeparator()
            .AppendName( fullTableName )
            .AppendTokenSeparator()
            .Append( "DROP COLUMN" )
            .AppendTokenSeparator()
            .AppendName( columnName )
            .AppendCommandEnd();
    }

    internal static StringBuilder AppendRenameColumn(this StringBuilder builder, string fullTableName, string oldName, string newName)
    {
        return builder
            .Append( "ALTER TABLE" )
            .AppendTokenSeparator()
            .AppendName( fullTableName )
            .AppendTokenSeparator()
            .Append( "RENAME COLUMN" )
            .AppendTokenSeparator()
            .AppendName( oldName )
            .AppendTokenSeparator()
            .Append( "TO" )
            .AppendTokenSeparator()
            .AppendName( newName )
            .AppendCommandEnd();
    }

    internal static StringBuilder AppendIndexedColumn(this StringBuilder builder, string name, OrderBy ordering)
    {
        return builder
            .AppendName( name )
            .AppendTokenSeparator()
            .Append( ordering.Name )
            .AppendElementSeparator()
            .AppendTokenSeparator();
    }

    internal static StringBuilder AppendNamedElement(this StringBuilder builder, string name)
    {
        return builder.AppendName( name ).AppendElementSeparator().AppendTokenSeparator();
    }

    internal static StringBuilder AppendPrimaryKeyDefinition(this StringBuilder builder, string fullPrimaryKeyName)
    {
        return builder.AppendConstraintDefinition( fullPrimaryKeyName, "PRIMARY KEY" );
    }

    internal static StringBuilder AppendForeignKeyDefinition(this StringBuilder builder, string fullForeignKeyName)
    {
        return builder.AppendConstraintDefinition( fullForeignKeyName, "FOREIGN KEY" );
    }

    internal static StringBuilder AppendForeignKeyReferenceDefinition(this StringBuilder builder, string fullTableName)
    {
        return builder.Append( "REFERENCES" ).AppendTokenSeparator().AppendName( fullTableName );
    }

    internal static StringBuilder AppendForeignKeyBehaviors(
        this StringBuilder builder,
        ReferenceBehavior onDelete,
        ReferenceBehavior onUpdate)
    {
        return builder
            .Append( "ON DELETE" )
            .AppendTokenSeparator()
            .Append( onDelete.Name )
            .AppendTokenSeparator()
            .Append( "ON UPDATE" )
            .AppendTokenSeparator()
            .Append( onUpdate.Name );
    }

    internal static StringBuilder AppendCreateTableBegin(this StringBuilder builder, string fullTableName)
    {
        return builder
            .Append( "CREATE TABLE" )
            .AppendTokenSeparator()
            .AppendName( fullTableName )
            .AppendTokenSeparator()
            .AppendElementsBegin()
            .AppendLine();
    }

    internal static StringBuilder AppendCreateTableEnd(this StringBuilder builder)
    {
        return builder
            .AppendLine()
            .AppendElementsEnd()
            .AppendTokenSeparator()
            .Append( "WITHOUT ROWID" )
            .AppendCommandEnd();
    }

    internal static StringBuilder AppendRenameTable(this StringBuilder builder, string oldFullTableName, string newFullTableName)
    {
        return builder
            .Append( "ALTER TABLE" )
            .AppendTokenSeparator()
            .AppendName( oldFullTableName )
            .AppendTokenSeparator()
            .Append( "RENAME TO" )
            .AppendTokenSeparator()
            .AppendName( newFullTableName )
            .AppendCommandEnd();
    }

    internal static StringBuilder AppendDropTable(this StringBuilder builder, string fullTableName)
    {
        return builder
            .Append( "DROP TABLE" )
            .AppendTokenSeparator()
            .AppendName( fullTableName )
            .AppendCommandEnd();
    }

    internal static StringBuilder AppendCreateIndexDefinition(
        this StringBuilder builder,
        string fullIndexName,
        string fullTableName,
        bool isUnique)
    {
        builder.Append( "CREATE" );

        if ( isUnique )
            builder.AppendTokenSeparator().Append( "UNIQUE" );

        return builder
            .AppendTokenSeparator()
            .Append( "INDEX" )
            .AppendTokenSeparator()
            .AppendName( fullIndexName )
            .AppendTokenSeparator()
            .Append( "ON" )
            .AppendTokenSeparator()
            .AppendName( fullTableName );
    }

    internal static StringBuilder AppendDropIndex(this StringBuilder builder, string indexFullName)
    {
        return builder
            .Append( "DROP INDEX" )
            .AppendTokenSeparator()
            .AppendName( indexFullName )
            .AppendCommandEnd();
    }

    internal static StringBuilder AppendInsertIntoBegin(this StringBuilder builder, string fullTableName)
    {
        return builder
            .Append( "INSERT INTO" )
            .AppendTokenSeparator()
            .AppendName( fullTableName )
            .AppendTokenSeparator()
            .AppendElementsBegin();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendSelect(this StringBuilder builder)
    {
        return builder.Append( "SELECT" );
    }

    internal static StringBuilder AppendFrom(this StringBuilder builder, string fullTableName)
    {
        return builder.Append( "FROM" ).AppendTokenSeparator().AppendName( fullTableName );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendDefaultValue(this StringBuilder builder, SqliteColumnTypeDefinition valueType, object? value)
    {
        return builder.Append( value is null ? "NULL" : valueType.TryToDbLiteral( value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static StringBuilder AppendCoalesceBegin(this StringBuilder builder)
    {
        return builder.AppendFunctionCallBegin( "COALESCE" );
    }

    internal static StringBuilder AppendCastAs(this StringBuilder builder, string columnName, string targetDataTypeName)
    {
        return builder
            .AppendFunctionCallBegin( "CAST" )
            .AppendName( columnName )
            .AppendTokenSeparator()
            .Append( "AS" )
            .AppendTokenSeparator()
            .Append( targetDataTypeName )
            .AppendElementsEnd();
    }

    internal static StringBuilder AppendCreateViewBegin(this StringBuilder builder, string fullViewName)
    {
        return builder
            .Append( "CREATE VIEW" )
            .AppendTokenSeparator()
            .AppendName( fullViewName )
            .AppendTokenSeparator()
            .Append( "AS" )
            .AppendLine();
    }

    internal static StringBuilder AppendDropView(this StringBuilder builder, string fullViewName)
    {
        return builder
            .Append( "DROP VIEW" )
            .AppendTokenSeparator()
            .AppendName( fullViewName )
            .AppendCommandEnd();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringBuilder AppendConstraintDefinition(this StringBuilder builder, string fullConstraintName, string type)
    {
        return builder
            .Append( "CONSTRAINT" )
            .AppendTokenSeparator()
            .AppendName( fullConstraintName )
            .AppendTokenSeparator()
            .Append( type );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static StringBuilder AppendFunctionCallBegin(this StringBuilder builder, string name)
    {
        return builder.Append( name ).AppendElementsBegin();
    }
}
