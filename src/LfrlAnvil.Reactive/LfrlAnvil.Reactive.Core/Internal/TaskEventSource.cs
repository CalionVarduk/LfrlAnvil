// Copyright 2024-2026 Łukasz Furlepa
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
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Internal;

/// <summary>
/// Represents a generic disposable event source that can be listened to,
/// that notifies its listeners with a single event when the created <see cref="Task{TResult}"/> completes,
/// and then disposes the listener. When an event subscriber gets disposed before the task completes,
/// then the underlying <see cref="CancellationTokenSource"/> will request cancellation.
/// </summary>
/// <typeparam name="TEvent">Event type.</typeparam>
public sealed class TaskEventSource<TEvent> : EventSource<FromTask<TEvent>>
{
    private readonly Func<CancellationToken, Task<TEvent>> _taskFactory;
    private readonly TaskSchedulerCapture _schedulerCapture;

    internal TaskEventSource(
        Func<CancellationToken, Task<TEvent>> taskFactory,
        TaskSchedulerCapture schedulerCapture)
    {
        _taskFactory = taskFactory;
        _schedulerCapture = schedulerCapture;
    }

    /// <inheritdoc />
    protected override IEventListener<FromTask<TEvent>> OverrideListenerUnsafe(
        IEventSubscriber subscriber,
        IEventListener<FromTask<TEvent>> listener)
    {
        using ( AcquireLock() )
        {
            if ( IsDisposedUnsafe() )
                return listener;
        }

        var callbackScheduler = _schedulerCapture.TryGetScheduler();
        return new EventListener( listener, subscriber, _taskFactory, callbackScheduler );
    }

    private sealed class EventListener : DecoratedEventListener<FromTask<TEvent>, FromTask<TEvent>>
    {
        private readonly IEventSubscriber _subscriber;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private InterlockedInt32 _disposalSource;

        internal EventListener(
            IEventListener<FromTask<TEvent>> next,
            IEventSubscriber subscriber,
            Func<CancellationToken, Task<TEvent>> taskFactory,
            TaskScheduler? callbackScheduler)
            : base( next )
        {
            _subscriber = subscriber;
            _disposalSource = new InterlockedInt32( 0 );
            _cancellationTokenSource = new CancellationTokenSource();

            var task = taskFactory( _cancellationTokenSource.Token );
            AddTaskContinuation( task, callbackScheduler );

            if ( task.Status == TaskStatus.Created )
                task.Start( TaskScheduler.Default );
        }

        public override void React(FromTask<TEvent> @event)
        {
            Next.React( @event );
        }

        public override void OnDispose(DisposalSource source)
        {
            _disposalSource.Write( ( int )source );
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private void AddTaskContinuation(Task<TEvent> task, TaskScheduler? scheduler)
        {
            if ( scheduler is not null )
                task.ContinueWith( ContinuationCallback, this, default, TaskContinuationOptions.ExecuteSynchronously, scheduler );
            else
                task.ContinueWith( ContinuationCallback, this, TaskContinuationOptions.ExecuteSynchronously );
        }

        private void Complete()
        {
            _subscriber.Dispose();
            Next.OnDispose( ( DisposalSource )_disposalSource.Value );
        }

        private static void ContinuationCallback(Task<TEvent> completedTask, object? state)
        {
            Assume.IsNotNull( state );
            var listener = ReinterpretCast.To<EventListener>( state );

            var nextEvent = new FromTask<TEvent>( completedTask );
            try
            {
                listener.React( nextEvent );
            }
            finally
            {
                listener.Complete();
            }
        }
    }
}
