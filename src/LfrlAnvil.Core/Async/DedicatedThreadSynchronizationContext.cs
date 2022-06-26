﻿using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Async
{
    public class DedicatedThreadSynchronizationContext : SynchronizationContext, IDisposable
    {
        private readonly BlockingCollection<Pair<SendOrPostCallback, object>> _queue;
        private readonly Thread _thread;
        private CultureInfo? _threadCulture;
        private CultureInfo? _threadUICulture;

        public DedicatedThreadSynchronizationContext(ThreadParams @params = default)
        {
            _queue = new BlockingCollection<Pair<SendOrPostCallback, object>>();
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

        public ThreadPriority ThreadPriority { get; }
        public CultureInfo ThreadCulture => _threadCulture ?? CultureInfo.InvariantCulture;
        public CultureInfo ThreadUICulture => _threadUICulture ?? CultureInfo.InvariantCulture;
        public int ThreadId => _thread.ManagedThreadId;
        public string? ThreadName => _thread.Name;
        public bool IsActive => _thread.IsAlive;

        public void Dispose()
        {
            _queue.CompleteAdding();
            _queue.Dispose();
        }

        public void JoinThread()
        {
            _thread.Join();
        }

        public sealed override void Post(SendOrPostCallback d, object state)
        {
            _queue.Add( Pair.Create( d, state ) );
        }

        public sealed override void Send(SendOrPostCallback d, object state)
        {
            using var reset = new ManualResetEventSlim( false );
            Post( SendCallback, new SendCallbackState( d, state, reset ) );
            reset.Wait();
        }

        private static void OnThreadStart(object threadState)
        {
            var context = (DedicatedThreadSynchronizationContext)threadState;
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

        private static void SendCallback(object state)
        {
            var @params = (SendCallbackState)state;
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
            internal readonly object CallbackState;
            internal readonly ManualResetEventSlim Reset;

            internal SendCallbackState(SendOrPostCallback callback, object callbackState, ManualResetEventSlim reset)
            {
                Callback = callback;
                CallbackState = callbackState;
                Reset = reset;
            }
        }
    }
}