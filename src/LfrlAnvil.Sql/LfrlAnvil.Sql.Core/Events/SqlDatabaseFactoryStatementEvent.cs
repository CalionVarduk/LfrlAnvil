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
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Events;

/// <summary>
/// Represents an event associates with an SQL statement that is to be ran during <see cref="ISqlDatabase"/> creation.
/// </summary>
/// <param name="Key">Event's identifier.</param>
/// <param name="Sql">SQL statement.</param>
/// <param name="Timeout">Command timeout.</param>
/// <param name="Parameters">Collection of (name, value) pairs representing SQL command parameters.</param>
/// <param name="Type">Type of this event.</param>
/// <param name="UtcStartDate">UTC date and time representing the start of this SQL statement's execution.</param>
public readonly record struct SqlDatabaseFactoryStatementEvent(
    SqlDatabaseFactoryStatementKey Key,
    string Sql,
    TimeSpan Timeout,
    IReadOnlyList<KeyValuePair<string?, object?>> Parameters,
    SqlDatabaseFactoryStatementType Type,
    DateTime UtcStartDate
)
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseFactoryStatementEvent"/> instance.
    /// </summary>
    /// <param name="command"><see cref="IDbCommand"/> instance to create this event from.</param>
    /// <param name="key">Event's identifier.</param>
    /// <param name="type">Type of this event.</param>
    /// <returns>New <see cref="SqlDatabaseFactoryStatementEvent"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlDatabaseFactoryStatementEvent Create(
        IDbCommand command,
        SqlDatabaseFactoryStatementKey key,
        SqlDatabaseFactoryStatementType type)
    {
        var parameters = GetCommandParameters( command );
        return new SqlDatabaseFactoryStatementEvent(
            key,
            command.CommandText,
            TimeSpan.FromSeconds( command.CommandTimeout ),
            parameters,
            type,
            DateTime.UtcNow );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static KeyValuePair<string?, object?>[] GetCommandParameters(IDbCommand command)
    {
        if ( command is not DbCommand dbCommand )
            return Array.Empty<KeyValuePair<string?, object?>>();

        var commandParameters = dbCommand.Parameters;
        var commandParameterCount = commandParameters.Count;
        if ( commandParameterCount == 0 )
            return Array.Empty<KeyValuePair<string?, object?>>();

        var result = new KeyValuePair<string?, object?>[commandParameterCount];
        for ( var i = 0; i < commandParameterCount; ++i )
        {
            var parameter = commandParameters[i];
            result[i] = KeyValuePair.Create( ( string? )parameter.ParameterName, parameter.Value );
        }

        return result;
    }
}
