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
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents an implementation of an object that can be wrapped by a <see cref="ValueTask{T}"/>.
/// </summary>
/// <typeparam name="TResult">The type of the result produced by the <see cref="ManualResetValueTaskSource{TResult}"/>.</typeparam>
public sealed class ManualResetValueTaskSource<TResult> : IValueTaskSource<TResult>
{
    private ManualResetValueTaskSourceCore<TResult> _core;

    /// <summary>
    /// Creates a new <see cref="ManualResetValueTaskSource{T}"/> instance.
    /// </summary>
    /// <param name="runContinuationsAsynchronously">
    /// Specifies whether or not to force continuations to run asynchronously. Equal to <b>true</b> by default.
    /// </param>
    public ManualResetValueTaskSource(bool runContinuationsAsynchronously = true)
    {
        _core = new ManualResetValueTaskSourceCore<TResult> { RunContinuationsAsynchronously = runContinuationsAsynchronously };
    }

    /// <summary>
    /// Specifies whether or not continuations are forced to run asynchronously.
    /// </summary>
    public bool RunContinuationsAsynchronously => _core.RunContinuationsAsynchronously;

    /// <summary>
    /// Specifies the status of this value task source's current operation.
    /// </summary>
    public ValueTaskSourceStatus Status => _core.GetStatus( _core.Version );

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        return $"{nameof( _core.Version )}: {_core.Version}, {nameof( Status )}: {Status}";
    }

    /// <summary>
    /// Completes this value task source's current operation with a successful result.
    /// </summary>
    /// <param name="result">Result of the operation.</param>
    /// <exception cref="InvalidOperationException">When current operation has already been completed.</exception>
    public void SetResult(TResult result)
    {
        _core.SetResult( result );
    }

    /// <summary>
    /// Completes this value task source's current operation with an error.
    /// </summary>
    /// <param name="error">Exception of the operation.</param>
    /// <exception cref="InvalidOperationException">When current operation has already been completed.</exception>
    public void SetException(Exception error)
    {
        _core.SetException( error );
    }

    /// <summary>
    /// Completes this value task source's current operation with a cancellation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token associated with the operation.</param>
    /// <exception cref="InvalidOperationException">When current operation has already been completed.</exception>
    public void SetCancelled(CancellationToken cancellationToken)
    {
        SetException( new OperationCanceledException( cancellationToken ) );
    }

    /// <summary>
    /// Resets this value task source in order to prepare it for the next operation.
    /// </summary>
    public void Reset()
    {
        _core.Reset();
    }

    /// <summary>
    /// Creates a <see cref="ValueTask{T}"/> instance from this value task source.
    /// </summary>
    /// <returns>New <see cref="ValueTask{T}"/> instance.</returns>
    /// <remarks>The returned value task will complete once this value task source is manually told to complete.</remarks>
    public ValueTask<TResult> GetTask()
    {
        return new ValueTask<TResult>( this, _core.Version );
    }

    TResult IValueTaskSource<TResult>.GetResult(short token)
    {
        return _core.GetResult( token );
    }

    ValueTaskSourceStatus IValueTaskSource<TResult>.GetStatus(short token)
    {
        return _core.GetStatus( token );
    }

    void IValueTaskSource<TResult>.OnCompleted(
        Action<object?> continuation,
        object? state,
        short token,
        ValueTaskSourceOnCompletedFlags flags)
    {
        _core.OnCompleted( continuation, state, token, flags );
    }
}
