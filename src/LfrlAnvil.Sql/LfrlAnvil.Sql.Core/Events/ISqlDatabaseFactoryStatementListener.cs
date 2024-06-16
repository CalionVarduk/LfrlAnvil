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

namespace LfrlAnvil.Sql.Events;

/// <summary>
/// Represents a listener that allows to react to SQL statements executed during <see cref="ISqlDatabase"/> creation.
/// </summary>
public interface ISqlDatabaseFactoryStatementListener
{
    /// <summary>
    /// Method invoked just before an SQL statement execution starts.
    /// </summary>
    /// <param name="event">Event that contains information about an SQL statement to be invoked.</param>
    void OnBefore(SqlDatabaseFactoryStatementEvent @event);

    /// <summary>
    /// Method invoked just after an SQL statement execution has finished.
    /// </summary>
    /// <param name="event">Event that contains information about an invoked SQL statement.</param>
    /// <param name="elapsedTime">Amount of time elapsed during an SQL statement's execution.</param>
    /// <param name="exception">Optional exception thrown during an SQL statement execution.</param>
    void OnAfter(SqlDatabaseFactoryStatementEvent @event, TimeSpan elapsedTime, Exception? exception);
}
