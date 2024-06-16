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
using System.Threading.Tasks;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an object capable of diagnosing SQL statements invoked through <see cref="IDbCommand"/> instances.
/// </summary>
/// <typeparam name="TCommand"><see cref="IDbCommand"/> type.</typeparam>
/// <typeparam name="TArgs">Delegate argument type.</typeparam>
public readonly struct DbCommandDiagnoser<TCommand, TArgs>
    where TCommand : IDbCommand
{
    /// <summary>
    /// Creates a new <see cref="DbCommandDiagnoser{TCommand,TArgs}"/> instance.
    /// </summary>
    /// <param name="beforeExecute">Optional delegate invoked just before an SQL statement execution starts.</param>
    /// <param name="afterExecute">Optional delegate invoked just after an SQL statement execution has finished.</param>
    public DbCommandDiagnoser(
        Action<TCommand, TArgs>? beforeExecute = null,
        Action<TCommand, TArgs, TimeSpan, Exception?>? afterExecute = null)
    {
        BeforeExecute = beforeExecute;
        AfterExecute = afterExecute;
    }

    /// <summary>
    /// Optional delegate invoked just before an SQL statement execution starts.
    /// </summary>
    public Action<TCommand, TArgs>? BeforeExecute { get; }

    /// <summary>
    /// Optional delegate invoked just after an SQL statement execution has finished.
    /// </summary>
    public Action<TCommand, TArgs, TimeSpan, Exception?>? AfterExecute { get; }

    /// <summary>
    /// Executes provided <see cref="IDbCommand"/> synchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="args"><see cref="BeforeExecute"/>/<see cref="AfterExecute"/> delegate arguments.</param>
    /// <param name="invoker">Delegate that invokes the provided <paramref name="command"/> and returns the result.</param>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>Result of the invocation.</returns>
    public TResult Execute<TResult>(TCommand command, TArgs args, Func<TCommand, TResult> invoker)
    {
        TResult result;
        var stopwatch = StopwatchSlim.Create();
        BeforeExecute?.Invoke( command, args );

        try
        {
            result = invoker( command );
        }
        catch ( Exception exc )
        {
            AfterExecute?.Invoke( command, args, stopwatch.ElapsedTime, exc );
            throw;
        }

        AfterExecute?.Invoke( command, args, stopwatch.ElapsedTime, null );
        return result;
    }

    /// <summary>
    /// Executes provided <see cref="IDbCommand"/> asynchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="args"><see cref="BeforeExecute"/>/<see cref="AfterExecute"/> delegate arguments.</param>
    /// <param name="invoker">Delegate that invokes the provided <paramref name="command"/> and returns the result.</param>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the result of the invocation.</returns>
    public async ValueTask<TResult> ExecuteAsync<TResult>(TCommand command, TArgs args, Func<TCommand, ValueTask<TResult>> invoker)
    {
        TResult result;
        var stopwatch = StopwatchSlim.Create();
        BeforeExecute?.Invoke( command, args );

        try
        {
            result = await invoker( command ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            AfterExecute?.Invoke( command, args, stopwatch.ElapsedTime, exc );
            throw;
        }

        AfterExecute?.Invoke( command, args, stopwatch.ElapsedTime, null );
        return result;
    }

    /// <summary>
    /// Executes provided <see cref="IDbCommand"/> asynchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="args"><see cref="BeforeExecute"/>/<see cref="AfterExecute"/> delegate arguments.</param>
    /// <param name="invoker">Delegate that invokes the provided <paramref name="command"/> and returns the result.</param>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the result of the invocation.</returns>
    public async ValueTask<TResult> ExecuteAsync<TResult>(TCommand command, TArgs args, Func<TCommand, Task<TResult>> invoker)
    {
        TResult result;
        var stopwatch = StopwatchSlim.Create();
        BeforeExecute?.Invoke( command, args );

        try
        {
            result = await invoker( command ).ConfigureAwait( false );
        }
        catch ( Exception exc )
        {
            AfterExecute?.Invoke( command, args, stopwatch.ElapsedTime, exc );
            throw;
        }

        AfterExecute?.Invoke( command, args, stopwatch.ElapsedTime, null );
        return result;
    }
}
