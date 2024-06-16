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
using System.Data;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a lightweight disposable object that swaps <see cref="IDbCommand"/> timeout.
/// </summary>
public readonly struct TemporaryCommandTimeout : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="TemporaryCommandTimeout"/> instance.
    /// </summary>
    /// <param name="command">Command to swap <see cref="IDbCommand.CommandTimeout"/> for.</param>
    /// <param name="timeout">Optional timeout to set. Lack of value will cause the timeout to not be changed.</param>
    /// <exception cref="ArgumentException">When <paramref name="timeout"/> is less <b>0</b>.</exception>
    public TemporaryCommandTimeout(IDbCommand command, TimeSpan? timeout)
    {
        Command = command;
        PreviousTimeout = Command.CommandTimeout;
        if ( timeout is not null )
            Command.CommandTimeout = ( int )Math.Ceiling( timeout.Value.TotalSeconds );
    }

    /// <summary>
    /// Underlying <see cref="IDbCommand"/> instance.
    /// </summary>
    public IDbCommand Command { get; }

    /// <summary>
    /// <see cref="IDbCommand.CommandTimeout"/> of <see cref="Command"/> before the change.
    /// </summary>
    public int PreviousTimeout { get; }

    /// <inheritdoc />
    /// <remarks>Sets <see cref="PreviousTimeout"/> as timeout of the <see cref="Command"/>.</remarks>
    public void Dispose()
    {
        Command.CommandTimeout = PreviousTimeout;
    }
}
