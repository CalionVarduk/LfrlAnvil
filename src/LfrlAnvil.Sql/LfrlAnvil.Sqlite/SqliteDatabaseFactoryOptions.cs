using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Represents available options for creating SQLite database objects through <see cref="SqliteDatabaseFactory"/>.
/// </summary>
public readonly struct SqliteDatabaseFactoryOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly SqliteDatabaseFactoryOptions Default = new SqliteDatabaseFactoryOptions();

    /// <summary>
    /// Default creator of <see cref="SqliteColumnTypeDefinitionProvider"/> instances.
    /// </summary>
    public static readonly SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>
        BaseTypeDefinitionsCreator = static (_, _) => new SqliteColumnTypeDefinitionProviderBuilder().Build();

    /// <summary>
    /// Default creator of <see cref="SqliteNodeInterpreterFactory"/> instances.
    /// </summary>
    public static readonly
        SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider, SqliteNodeInterpreterFactory>
        BaseNodeInterpretersCreator = static (_, _, _, typeDefinitions) =>
            new SqliteNodeInterpreterFactory( SqliteNodeInterpreterOptions.Default.SetTypeDefinitions( typeDefinitions ) );

    private readonly SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? _defaultNamesCreator;

    private readonly SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>?
        _typeDefinitionsCreator;

    private readonly
        SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider, SqliteNodeInterpreterFactory>?
        _nodeInterpretersCreator;

    private SqliteDatabaseFactoryOptions(
        bool isConnectionPermanent,
        bool areForeignKeyChecksDisabled,
        SqliteDatabaseEncoding? encoding,
        SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? defaultNamesCreator,
        SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>? typeDefinitionsCreator,
        SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider, SqliteNodeInterpreterFactory>?
            nodeInterpretersCreator)
    {
        IsConnectionPermanent = isConnectionPermanent;
        AreForeignKeyChecksDisabled = areForeignKeyChecksDisabled;
        Encoding = encoding;
        _defaultNamesCreator = defaultNamesCreator;
        _typeDefinitionsCreator = typeDefinitionsCreator;
        _nodeInterpretersCreator = nodeInterpretersCreator;
    }

    /// <summary>
    /// Specifies whether or not the DB connection should stay permanently connected.
    /// </summary>
    /// <remarks>
    /// Enabling this option will cause the normal disposal of the <see cref="SqliteConnection"/> instance
    /// associated with the <see cref="SqliteDatabase.Connector"/> to do nothing.
    /// The connection will be closed and disposed only when the whole <see cref="SqliteDatabase"/> instance is disposed.
    /// </remarks>
    public bool IsConnectionPermanent { get; }

    /// <summary>
    /// Specifies whether or not foreign key checks are disabled.
    /// </summary>
    /// <remarks>
    /// Foreign key constraint validity is checked in <see cref="SqlDatabaseCreateMode.Commit"/> mode only,
    /// after all pending SQL statements of a single <see cref="ISqlDatabaseVersion"/> have been applied
    /// but before the DB transaction is committed. During the check, the <b>PRAGMA foreign_key_check(TABLE_NAME)</b> statement is ran
    /// for each <see cref="SqliteTableBuilder"/> instance that has been marked as created or modified.
    /// </remarks>
    public bool AreForeignKeyChecksDisabled { get; }

    /// <summary>
    /// Specifies optional <see cref="SqliteDatabaseEncoding"/> of created databases.
    /// </summary>
    public SqliteDatabaseEncoding? Encoding { get; }

    /// <summary>
    /// Specifies the creator of <see cref="SqlDefaultObjectNameProvider"/> instances.
    /// </summary>
    public SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =>
        _defaultNamesCreator ?? SqlHelpers.DefaultNamesCreator;

    /// <summary>
    /// Specifies the creator of <see cref="SqliteColumnTypeDefinitionProvider"/> instances.
    /// </summary>
    public SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider> TypeDefinitionsCreator =>
        _typeDefinitionsCreator ?? BaseTypeDefinitionsCreator;

    /// <summary>
    /// Specifies the creator of <see cref="SqliteNodeInterpreterFactory"/> instances.
    /// </summary>
    public SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider, SqliteNodeInterpreterFactory>
        NodeInterpretersCreator =>
        _nodeInterpretersCreator ?? BaseNodeInterpretersCreator;

    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactoryOptions"/> instance with changed <see cref="IsConnectionPermanent"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteDatabaseFactoryOptions EnableConnectionPermanence(bool enabled = true)
    {
        return new SqliteDatabaseFactoryOptions(
            enabled,
            AreForeignKeyChecksDisabled,
            Encoding,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactoryOptions"/> instance with changed <see cref="AreForeignKeyChecksDisabled"/>.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteDatabaseFactoryOptions EnableForeignKeyChecks(bool enabled = true)
    {
        return new SqliteDatabaseFactoryOptions(
            IsConnectionPermanent,
            ! enabled,
            Encoding,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactoryOptions"/> instance with changed <see cref="Encoding"/>.
    /// </summary>
    /// <param name="value">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns>New <see cref="SqliteDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteDatabaseFactoryOptions SetEncoding(SqliteDatabaseEncoding? value)
    {
        if ( value is not null )
            Ensure.IsDefined( value.Value );

        return new SqliteDatabaseFactoryOptions(
            IsConnectionPermanent,
            AreForeignKeyChecksDisabled,
            value,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactoryOptions"/> instance with changed <see cref="DefaultNamesCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="SqliteDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteDatabaseFactoryOptions SetDefaultNamesCreator(SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider>? creator)
    {
        return new SqliteDatabaseFactoryOptions(
            IsConnectionPermanent,
            AreForeignKeyChecksDisabled,
            Encoding,
            creator,
            _typeDefinitionsCreator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactoryOptions"/> instance with changed <see cref="TypeDefinitionsCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="SqliteDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteDatabaseFactoryOptions SetTypeDefinitionsCreator(
        SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>? creator)
    {
        return new SqliteDatabaseFactoryOptions(
            IsConnectionPermanent,
            AreForeignKeyChecksDisabled,
            Encoding,
            _defaultNamesCreator,
            creator,
            _nodeInterpretersCreator );
    }

    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactoryOptions"/> instance with changed <see cref="NodeInterpretersCreator"/>.
    /// </summary>
    /// <param name="creator">Value to set.</param>
    /// <returns>New <see cref="SqliteDatabaseFactoryOptions"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqliteDatabaseFactoryOptions SetNodeInterpretersCreator(
        SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider, SqliteNodeInterpreterFactory>? creator)
    {
        return new SqliteDatabaseFactoryOptions(
            IsConnectionPermanent,
            AreForeignKeyChecksDisabled,
            Encoding,
            _defaultNamesCreator,
            _typeDefinitionsCreator,
            creator );
    }
}
