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
using LfrlAnvil.Sql.Exceptions;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a tracker of changes applied to the attached database builder.
/// </summary>
public interface ISqlDatabaseChangeTracker
{
    /// <summary>
    /// Represents a currently tracked SQL object builder instance, whose changes will be aggregated.
    /// </summary>
    ISqlObjectBuilder? ActiveObject { get; }

    /// <summary>
    /// Specifies the existence state of <see cref="ActiveObject"/>.
    /// </summary>
    SqlObjectExistenceState ActiveObjectExistenceState { get; }

    /// <summary>
    /// Optional <see cref="IDbCommand.CommandTimeout"/> for registered <see cref="SqlDatabaseBuilderCommandAction"/> instances.
    /// </summary>
    TimeSpan? ActionTimeout { get; }

    /// <summary>
    /// Specifies the current <see cref="SqlDatabaseCreateMode"/>.
    /// </summary>
    /// <remarks>
    /// Change tracker will ignore all changes when <see cref="Mode"/> is equal to <see cref="SqlDatabaseCreateMode.NoChanges"/>.
    /// </remarks>
    SqlDatabaseCreateMode Mode { get; }

    /// <summary>
    /// Specifies whether or not this change tracker is attached.
    /// </summary>
    /// <remarks>Change tracker will ignore all changes when <see cref="IsAttached"/> is equal to <b>false</b>.</remarks>
    bool IsAttached { get; }

    /// <summary>
    /// Database that this instance tracks.
    /// </summary>
    ISqlDatabaseBuilder Database { get; }

    /// <summary>
    /// Returns a collection of all pending <see cref="SqlDatabaseBuilderCommandAction"/> instances.
    /// </summary>
    /// <returns>Collection of all pending <see cref="SqlDatabaseBuilderCommandAction"/> instances.</returns>
    ReadOnlySpan<SqlDatabaseBuilderCommandAction> GetPendingActions();

    /// <summary>
    /// Returns <see cref="SqlObjectExistenceState"/> value for the given <see cref="ISqlObjectBuilder"/> instance.
    /// </summary>
    /// <param name="target">Object to check.</param>
    /// <returns>
    /// One of three possible values:
    /// <list type="bullet">
    /// <item><description>
    /// <see cref="SqlObjectExistenceState.Unchanged"/> when <paramref name="target"/> does not exist
    /// or a pending action for its creation or removal does not exist,
    /// </description></item>
    /// <item><description>
    /// <see cref="SqlObjectExistenceState.Created"/> when pending creation action for <paramref name="target"/> exists,
    /// </description></item>
    /// <item><description>
    /// <see cref="SqlObjectExistenceState.Removed"/> when pending removal action for <paramref name="target"/> exists.
    /// </description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Use <see cref="ActiveObjectExistenceState"/> instead, for reading current state of the <see cref="ActiveObject"/>.
    /// </remarks>
    [Pure]
    SqlObjectExistenceState GetExistenceState(ISqlObjectBuilder target);

    /// <summary>
    /// Checks whether or not a change for the given <paramref name="target"/>
    /// and its property's <paramref name="descriptor"/> is currently pending.
    /// </summary>
    /// <param name="target">Object to check.</param>
    /// <param name="descriptor">Property change descriptor.</param>
    /// <returns><b>true</b> when the change is pending, otherwise <b>false</b>.</returns>
    [Pure]
    bool ContainsChange(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor);

    /// <summary>
    /// Attempts to return a value before the change associated with the given <paramref name="target"/>
    /// and its property's <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="target">Object to check.</param>
    /// <param name="descriptor">Property change descriptor.</param>
    /// <param name="result"><b>out</b> parameter that returns the value before the change was made.</param>
    /// <returns><b>true</b> when the change is pending and original value was retrieved, otherwise <b>false</b>.</returns>
    bool TryGetOriginalValue(ISqlObjectBuilder target, SqlObjectChangeDescriptor descriptor, out object? result);

    /// <summary>
    /// Adds a custom <see cref="SqlDatabaseBuilderCommandAction"/> to this change tracker.
    /// </summary>
    /// <param name="action">Custom delegate that executes the <see cref="IDbCommand"/>.</param>
    /// <param name="setup">
    /// Optional delegate that prepares the <see cref="IDbCommand"/>. Can be used to e.g. prepare a collection of parameters.
    /// Equal to null by default.
    /// </param>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseChangeTracker AddAction(Action<IDbCommand> action, Action<IDbCommand>? setup = null);

    /// <summary>
    /// Adds a custom SQL <paramref name="statement"/> to this change tracker.
    /// </summary>
    /// <param name="statement">SQL statement node to add.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="SqlObjectBuilderException">When <paramref name="statement"/> contains parameters.</exception>
    ISqlDatabaseChangeTracker AddStatement(ISqlStatementNode statement);

    /// <summary>
    /// Adds a custom parameterized SQL <paramref name="statement"/> to this change tracker.
    /// </summary>
    /// <param name="statement">SQL statement node to add.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseChangeTracker AddParameterizedStatement(
        ISqlStatementNode statement,
        IEnumerable<SqlParameter> parameters,
        SqlParameterBinderCreationOptions? options = null);

    /// <summary>
    /// Adds a custom parameterized SQL <paramref name="statement"/> to this change tracker.
    /// </summary>
    /// <param name="statement">SQL statement node to add.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <param name="options">Optional <see cref="SqlParameterBinderCreationOptions"/>.</param>
    /// <typeparam name="TSource">Parameter source type.</typeparam>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseChangeTracker AddParameterizedStatement<TSource>(
        ISqlStatementNode statement,
        TSource parameters,
        SqlParameterBinderCreationOptions? options = null)
        where TSource : notnull;

    /// <summary>
    /// Changes <see cref="IsAttached"/> value for this change tracker.
    /// </summary>
    /// <param name="enabled">Value to set. Equal to <b>true</b> by default.</param>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseChangeTracker Attach(bool enabled = true);

    /// <summary>
    /// Completes all pending changes and registers appropriate <see cref="SqlDatabaseBuilderCommandAction"/> instances for them.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseChangeTracker CompletePendingChanges();

    /// <summary>
    /// Changes <see cref="ActionTimeout"/> value for this change tracker.
    /// </summary>
    /// <param name="value">Value to set.</param>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseChangeTracker SetActionTimeout(TimeSpan? value);
}
