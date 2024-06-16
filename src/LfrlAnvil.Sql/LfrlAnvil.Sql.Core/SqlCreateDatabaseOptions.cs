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
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Sql.Events;

namespace LfrlAnvil.Sql;

/// <summary>
/// Specifies available <see cref="ISqlDatabase"/> creation options.
/// </summary>
public readonly record struct SqlCreateDatabaseOptions
{
    /// <summary>
    /// Represents default options.
    /// </summary>
    public static readonly SqlCreateDatabaseOptions Default = new SqlCreateDatabaseOptions();

    /// <summary>
    /// <see cref="SqlDatabaseCreateMode"/> value that specifies the mode.
    /// </summary>
    public readonly SqlDatabaseCreateMode Mode;

    /// <summary>
    /// Specifies the name of the table that stores information about applied versions.
    /// </summary>
    public readonly SqlSchemaObjectName? VersionHistoryName;

    /// <summary>
    /// <see cref="SqlDatabaseVersionHistoryMode"/> value that specifies which versions should be inserted into the version history table.
    /// </summary>
    public readonly SqlDatabaseVersionHistoryMode VersionHistoryPersistenceMode;

    /// <summary>
    /// <see cref="SqlDatabaseVersionHistoryMode"/> value that specifies which version history records
    /// should be read in order to verify and find the next version to be applied.
    /// </summary>
    public readonly SqlDatabaseVersionHistoryMode VersionHistoryQueryMode;

    /// <summary>
    /// Specifies <see cref="IDbCommand"/> timeout for statements ran by the database factory.
    /// </summary>
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

    /// <summary>
    /// Returns a collection of registered <see cref="ISqlDatabaseFactoryStatementListener"/> instances.
    /// </summary>
    /// <returns>Collection of registered <see cref="ISqlDatabaseFactoryStatementListener"/> instances.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<ISqlDatabaseFactoryStatementListener> GetStatementListeners()
    {
        return CollectionsMarshal.AsSpan( _statementListeners ).Slice( 0, _statementListenerCount );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseOptions"/> instance with changed <see cref="Mode"/>.
    /// </summary>
    /// <param name="mode">Value to set.</param>
    /// <returns>New <see cref="SqlCreateDatabaseOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseOptions"/> instance with changed <see cref="VersionHistoryName"/>.
    /// </summary>
    /// <param name="name">Value to set.</param>
    /// <returns>New <see cref="SqlCreateDatabaseOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseOptions"/> instance with changed <see cref="VersionHistoryPersistenceMode"/>.
    /// </summary>
    /// <param name="mode">Value to set.</param>
    /// <returns>New <see cref="SqlCreateDatabaseOptions"/> instance.</returns>
    [Pure]
    public SqlCreateDatabaseOptions SetVersionHistoryPersistenceMode(SqlDatabaseVersionHistoryMode mode)
    {
        Ensure.IsDefined( mode );
        return new SqlCreateDatabaseOptions( Mode, VersionHistoryName, mode, VersionHistoryQueryMode, CommandTimeout, _statementListeners );
    }

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseOptions"/> instance with changed <see cref="VersionHistoryQueryMode"/>.
    /// </summary>
    /// <param name="mode">Value to set.</param>
    /// <returns>New <see cref="SqlCreateDatabaseOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseOptions"/> instance with changed <see cref="CommandTimeout"/>.
    /// </summary>
    /// <param name="timeout">Value to set.</param>
    /// <returns>New <see cref="SqlCreateDatabaseOptions"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="SqlCreateDatabaseOptions"/> instance
    /// with added <see cref="ISqlDatabaseFactoryStatementListener"/> instance.
    /// </summary>
    /// <param name="listener">Listener to add.</param>
    /// <returns>New <see cref="SqlCreateDatabaseOptions"/> instance.</returns>
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
