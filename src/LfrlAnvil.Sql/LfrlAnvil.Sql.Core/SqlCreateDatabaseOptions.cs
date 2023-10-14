using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Events;

namespace LfrlAnvil.Sql;

public readonly record struct SqlCreateDatabaseOptions
{
    public static readonly SqlCreateDatabaseOptions Default = new SqlCreateDatabaseOptions();

    public readonly SqlDatabaseCreateMode Mode;
    public readonly string? VersionHistorySchemaName;
    public readonly string? VersionHistoryTableName;
    public readonly SqlDatabaseVersionHistoryPersistenceMode VersionHistoryPersistenceMode;

    private readonly List<ISqlDatabaseFactoryStatementListener>? _statementListeners;
    private readonly int _statementListenerCount;

    private SqlCreateDatabaseOptions(
        SqlDatabaseCreateMode mode,
        string? versionHistorySchemaName,
        string? versionHistoryTableName,
        SqlDatabaseVersionHistoryPersistenceMode versionHistoryPersistenceMode,
        List<ISqlDatabaseFactoryStatementListener>? statementListeners)
    {
        Mode = mode;
        VersionHistorySchemaName = versionHistorySchemaName;
        VersionHistoryTableName = versionHistoryTableName;
        VersionHistoryPersistenceMode = versionHistoryPersistenceMode;
        _statementListeners = statementListeners;
        _statementListenerCount = statementListeners?.Count ?? 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<ISqlDatabaseFactoryStatementListener> GetStatementListeners()
    {
        return CollectionsMarshal.AsSpan( _statementListeners ).Slice( 0, _statementListenerCount );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetMode(SqlDatabaseCreateMode mode)
    {
        Ensure.IsDefined( mode, nameof( mode ) );
        return new SqlCreateDatabaseOptions(
            mode,
            VersionHistorySchemaName,
            VersionHistoryTableName,
            VersionHistoryPersistenceMode,
            _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistorySchemaName(string? name)
    {
        return new SqlCreateDatabaseOptions( Mode, name, VersionHistoryTableName, VersionHistoryPersistenceMode, _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryTableName(string? name)
    {
        return new SqlCreateDatabaseOptions( Mode, VersionHistorySchemaName, name, VersionHistoryPersistenceMode, _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryPersistenceMode(SqlDatabaseVersionHistoryPersistenceMode mode)
    {
        Ensure.IsDefined( mode, nameof( mode ) );
        return new SqlCreateDatabaseOptions( Mode, VersionHistorySchemaName, VersionHistoryTableName, mode, _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions AddStatementListener(ISqlDatabaseFactoryStatementListener listener)
    {
        var listeners = _statementListeners ?? new List<ISqlDatabaseFactoryStatementListener>();
        listeners.Add( listener );

        return new SqlCreateDatabaseOptions(
            Mode,
            VersionHistorySchemaName,
            VersionHistoryTableName,
            VersionHistoryPersistenceMode,
            listeners );
    }
}
