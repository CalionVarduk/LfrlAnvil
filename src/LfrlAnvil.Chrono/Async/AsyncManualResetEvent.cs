// Copyright 2025 Łukasz Furlepa
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
using System.Threading.Tasks;

namespace LfrlAnvil.Chrono.Async;

/// <summary>
/// Represents an asynchronous thread synchronization event that, when signaled, must be reset manually.
/// This event supports at most one active awaiter.
/// </summary>
public readonly struct AsyncManualResetEvent : IDisposable
{
    private readonly ValueTaskDelaySource.Node? _node;
    private readonly uint _version;

    internal AsyncManualResetEvent(ValueTaskDelaySource.Node node, uint version)
    {
        _node = node;
        _version = version;
    }

    /// <summary>
    /// Specifies <see cref="ValueTaskDelaySource"/> instance that backs this manual reset event.
    /// </summary>
    public ValueTaskDelaySource? Owner => _node?.Source;

    /// <inheritdoc/>
    /// <remarks>Causes awaited task to return <see cref="AsyncManualResetEventResult.Disposed"/> result.</remarks>
    public void Dispose()
    {
        _node?.OnResetEventDispose( _version );
    }

    /// <summary>
    /// Creates a new task that does not complete until this manual reset event is set.
    /// </summary>
    /// <param name="delay">Optional wait timeout. Disabled by default.</param>
    /// <returns>New <see cref="ValueTask{TResult}"/> instance which returns a <see cref="AsyncManualResetEventResult"/> value.</returns>
    /// <remarks>
    /// Returned task will complete immediately, when this event is already signaled or disposed or being awaited by another task.
    /// </remarks>
    public async ValueTask<AsyncManualResetEventResult> WaitAsync(Duration? delay = null)
    {
        if ( _node is null )
            return AsyncManualResetEventResult.Disposed;

        var success = false;
        try
        {
            var result = await _node.OnResetEventWait( _version, ref success, delay );
            return ( AsyncManualResetEventResult )result;
        }
        finally
        {
            if ( success )
                _node.OnResetEventTaskAwaitFinished( _version );
        }
    }

    /// <summary>
    /// Sets the state of this manual reset event to signaled,
    /// which causes awaited task to return <see cref="AsyncManualResetEventResult.Signaled"/> result.
    /// </summary>
    /// <returns><b>true</b>, when operation was successful, otherwise <b>false</b>.</returns>
    public bool Set()
    {
        return _node is not null && _node.OnResetEventSet( _version );
    }

    /// <summary>
    /// Sets the state of this manual reset event to non-signaled, which causes awaited task to not complete.
    /// </summary>
    /// <returns><b>true</b>, when operation was successful, otherwise <b>false</b>.</returns>
    public bool Reset()
    {
        return _node is not null && _node.OnResetEventReset( _version );
    }
}
