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
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents a <see cref="SynchronizationContext"/> with a dedicated underlying <see cref="Thread"/>.
/// </summary>
public sealed class DedicatedThreadSynchronizationContext : SynchronizationContext, IDisposable
{
    private readonly BlockingCollection<Pair<SendOrPostCallback, object?>> _queue;
    private readonly Thread _thread;
    private CultureInfo? _threadCulture;
    private CultureInfo? _threadUICulture;

    /// <summary>
    /// Creates a new <see cref="DedicatedThreadSynchronizationContext"/> instance.
    /// </summary>
    /// <param name="params">Optional parameters for the underlying thread.</param>
    public DedicatedThreadSynchronizationContext(ThreadParams @params = default)
    {
        _queue = new BlockingCollection<Pair<SendOrPostCallback, object?>>();
        _threadCulture = @params.Culture;
        _threadUICulture = @params.UICulture;
        ThreadPriority = @params.Priority ?? ThreadPriority.Normal;

        _thread = new Thread( OnThreadStart )
        {
            IsBackground = true,
            Name = @params.Name,
            Priority = ThreadPriority
        };

        _thread.Start( this );
    }

    /// <summary>
    /// Value indicating the scheduling priority of the underlying thread.
    /// </summary>
    public ThreadPriority ThreadPriority { get; }

    /// <summary>
    /// Culture of the underlying thread.
    /// </summary>
    public CultureInfo ThreadCulture => _threadCulture ?? CultureInfo.InvariantCulture;

    /// <summary>
    /// UI culture of the underlying thread.
    /// </summary>
    public CultureInfo ThreadUICulture => _threadUICulture ?? CultureInfo.InvariantCulture;

    /// <summary>
    /// Unique identifier of the underlying thread.
    /// </summary>
    public int ThreadId => _thread.ManagedThreadId;

    /// <summary>
    /// Name of the underlying thread.
    /// </summary>
    public string? ThreadName => _thread.Name;

    /// <summary>
    /// Value indicating the execution status of the underlying thread.
    /// </summary>
    public bool IsActive => _thread.IsAlive;

    /// <inheritdoc />
    public void Dispose()
    {
        _queue.CompleteAdding();
        _queue.Dispose();
    }

    /// <summary>
    /// Joins current thread to the underlying thread of this synchronization context.
    /// </summary>
    public void JoinThread()
    {
        _thread.Join();
    }

    /// <inheritdoc />
    public override void Post(SendOrPostCallback d, object? state)
    {
        _queue.Add( Pair.Create( d, state ) );
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        using var reset = new ManualResetEventSlim( false );
        Post( SendCallback, new SendCallbackState( d, state, reset ) );
        reset.Wait();
    }

    private static void OnThreadStart(object? threadState)
    {
        Assume.IsNotNull( threadState );
        var context = ReinterpretCast.To<DedicatedThreadSynchronizationContext>( threadState );
        SetSynchronizationContext( context );

        if ( context._threadCulture is not null )
            context._thread.CurrentCulture = context._threadCulture;
        else
            context._threadCulture = context._thread.CurrentCulture;

        if ( context._threadUICulture is not null )
            context._thread.CurrentUICulture = context._threadUICulture;
        else
            context._threadUICulture = context._thread.CurrentUICulture;

        try
        {
            if ( context._queue.IsAddingCompleted )
                return;
        }
        catch ( ObjectDisposedException )
        {
            return;
        }

        foreach ( var (callback, state) in context._queue.GetConsumingEnumerable() )
            callback( state );
    }

    private static void SendCallback(object? state)
    {
        Assume.IsNotNull( state );
        var @params = ReinterpretCast.To<SendCallbackState>( state );
        try
        {
            @params.Callback( @params.CallbackState );
        }
        finally
        {
            @params.Reset.Set();
        }
    }

    private sealed class SendCallbackState
    {
        internal readonly SendOrPostCallback Callback;
        internal readonly object? CallbackState;
        internal readonly ManualResetEventSlim Reset;

        internal SendCallbackState(SendOrPostCallback callback, object? callbackState, ManualResetEventSlim reset)
        {
            Callback = callback;
            CallbackState = callbackState;
            Reset = reset;
        }
    }
}
