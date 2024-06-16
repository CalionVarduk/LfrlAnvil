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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Events;

/// <summary>
/// Creates instances of <see cref="ISqlDatabaseFactoryStatementListener"/> type.
/// </summary>
public static class SqlDatabaseFactoryStatementListener
{
    /// <summary>
    /// Creates a new <see cref="ISqlDatabaseFactoryStatementListener"/> instance.
    /// </summary>
    /// <param name="onBefore">Delegate invoked just before an SQL statement execution starts.</param>
    /// <param name="onAfter">Delegate invoked just after an SQL statement execution has finished.</param>
    /// <returns>New <see cref="ISqlDatabaseFactoryStatementListener"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ISqlDatabaseFactoryStatementListener Create(
        Action<SqlDatabaseFactoryStatementEvent>? onBefore,
        Action<SqlDatabaseFactoryStatementEvent, TimeSpan, Exception?>? onAfter)
    {
        return new SqlDatabaseFactoryStatementLambdaListener( onBefore, onAfter );
    }

    private sealed class SqlDatabaseFactoryStatementLambdaListener : ISqlDatabaseFactoryStatementListener
    {
        private readonly Action<SqlDatabaseFactoryStatementEvent>? _onBefore;
        private readonly Action<SqlDatabaseFactoryStatementEvent, TimeSpan, Exception?>? _onAfter;

        internal SqlDatabaseFactoryStatementLambdaListener(
            Action<SqlDatabaseFactoryStatementEvent>? onBefore,
            Action<SqlDatabaseFactoryStatementEvent, TimeSpan, Exception?>? onAfter)
        {
            _onBefore = onBefore;
            _onAfter = onAfter;
        }

        public void OnBefore(SqlDatabaseFactoryStatementEvent @event)
        {
            _onBefore?.Invoke( @event );
        }

        public void OnAfter(SqlDatabaseFactoryStatementEvent @event, TimeSpan elapsedTime, Exception? exception)
        {
            _onAfter?.Invoke( @event, elapsedTime, exception );
        }
    }
}
