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
    public readonly SqlSchemaObjectName? VersionHistoryName;
    public readonly SqlDatabaseVersionHistoryMode VersionHistoryPersistenceMode;
    public readonly SqlDatabaseVersionHistoryMode VersionHistoryQueryMode;
    public readonly TimeSpan? CommandTimeout;

    private readonly List<ISqlDatabaseFactoryStatementListener>? _statementListeners;
    private readonly int _statementListenerCount;

    private SqlCreateDatabaseOptions(
        SqlDatabaseCreateMode mode,
        SqlSchemaObjectName? versionHistoryName,
        SqlDatabaseVersionHistoryMode versionHistoryPersistenceMode,
        SqlDatabaseVersionHistoryMode versionHistoryQueryMode,
        TimeSpan? commandTimeout,
        List<ISqlDatabaseFactoryStatementListener>? statementListeners)
    {
        Mode = mode;
        VersionHistoryName = versionHistoryName;
        VersionHistoryPersistenceMode = versionHistoryPersistenceMode;
        VersionHistoryQueryMode = versionHistoryQueryMode;
        CommandTimeout = commandTimeout;
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
        Ensure.IsDefined( mode );
        return new SqlCreateDatabaseOptions(
            mode,
            VersionHistoryName,
            VersionHistoryPersistenceMode,
            VersionHistoryQueryMode,
            CommandTimeout,
            _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryName(SqlSchemaObjectName? name)
    {
        return new SqlCreateDatabaseOptions(
            Mode,
            name,
            VersionHistoryPersistenceMode,
            VersionHistoryQueryMode,
            CommandTimeout,
            _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryPersistenceMode(SqlDatabaseVersionHistoryMode mode)
    {
        Ensure.IsDefined( mode );
        return new SqlCreateDatabaseOptions( Mode, VersionHistoryName, mode, VersionHistoryQueryMode, CommandTimeout, _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryQueryMode(SqlDatabaseVersionHistoryMode mode)
    {
        Ensure.IsDefined( mode );
        return new SqlCreateDatabaseOptions(
            Mode,
            VersionHistoryName,
            VersionHistoryPersistenceMode,
            mode,
            CommandTimeout,
            _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions SetCommandTimeout(TimeSpan? timeout)
    {
        return new SqlCreateDatabaseOptions(
            Mode,
            VersionHistoryName,
            VersionHistoryPersistenceMode,
            VersionHistoryQueryMode,
            timeout,
            _statementListeners );
    }

    [Pure]
    public SqlCreateDatabaseOptions AddStatementListener(ISqlDatabaseFactoryStatementListener listener)
    {
        var listeners = _statementListeners ?? new List<ISqlDatabaseFactoryStatementListener>();
        listeners.Add( listener );
        return new SqlCreateDatabaseOptions(
            Mode,
            VersionHistoryName,
            VersionHistoryPersistenceMode,
            VersionHistoryQueryMode,
            CommandTimeout,
            listeners );
    }
}
