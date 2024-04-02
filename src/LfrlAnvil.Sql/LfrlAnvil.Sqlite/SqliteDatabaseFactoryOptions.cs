﻿using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sqlite;

public readonly struct SqliteDatabaseFactoryOptions
{
    public static readonly SqliteDatabaseFactoryOptions Default = new SqliteDatabaseFactoryOptions();

    public static readonly SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider>
        BaseTypeDefinitionsCreator = static (_, _) => new SqliteColumnTypeDefinitionProviderBuilder().Build();

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

    public bool IsConnectionPermanent { get; }
    public bool AreForeignKeyChecksDisabled { get; }
    public SqliteDatabaseEncoding? Encoding { get; }

    public SqlDefaultObjectNameProviderCreator<SqlDefaultObjectNameProvider> DefaultNamesCreator =>
        _defaultNamesCreator ?? SqlHelpers.DefaultNamesCreator;

    public SqlColumnTypeDefinitionProviderCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider> TypeDefinitionsCreator =>
        _typeDefinitionsCreator ?? BaseTypeDefinitionsCreator;

    public SqlNodeInterpreterFactoryCreator<SqliteDataTypeProvider, SqliteColumnTypeDefinitionProvider, SqliteNodeInterpreterFactory>
        NodeInterpretersCreator =>
        _nodeInterpretersCreator ?? BaseNodeInterpretersCreator;

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
