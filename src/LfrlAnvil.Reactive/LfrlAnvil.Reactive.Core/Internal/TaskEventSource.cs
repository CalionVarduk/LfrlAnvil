using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Internal
{
    internal sealed class TaskEventSource<TEvent> : EventSource<FromTask<TEvent>>
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

        protected override IEventListener<FromTask<TEvent>> OverrideListener(
            IEventSubscriber subscriber,
            IEventListener<FromTask<TEvent>> listener)
        {
            if ( IsDisposed )
                return listener;

            var callbackScheduler = _schedulerCapture.TryGetScheduler();
            return new EventListener( listener, subscriber, _taskFactory, callbackScheduler );
        }

        private sealed class EventListener : DecoratedEventListener<FromTask<TEvent>, FromTask<TEvent>>
        {
            private readonly IEventSubscriber _subscriber;
            private readonly CancellationTokenSource _cancellationTokenSource;
            private DisposalSource _disposalSource;

            internal EventListener(
                IEventListener<FromTask<TEvent>> next,
                IEventSubscriber subscriber,
                Func<CancellationToken, Task<TEvent>> taskFactory,
                TaskScheduler? callbackScheduler)
                : base( next )
            {
                _subscriber = subscriber;
                _disposalSource = default;
                _cancellationTokenSource = new CancellationTokenSource();

                var task = taskFactory( _cancellationTokenSource.Token );
                AddTaskContinuation( task, callbackScheduler );

                if ( task.Status == TaskStatus.Created )
                    task.Start( TaskScheduler.Default );
            }

            public override void React(FromTask<TEvent> _) { }

            public override void OnDispose(DisposalSource source)
            {
                _disposalSource = source;
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }

            private void AddTaskContinuation(Task<TEvent> task, TaskScheduler? scheduler)
            {
                if ( scheduler is not null )
                {
                    task.ContinueWith( ContinuationCallback, scheduler );
                    return;
                }

                task.ContinueWith( ContinuationCallback );
            }

            private void ContinuationCallback(Task<TEvent> completedTask)
            {
                var nextEvent = new FromTask<TEvent>( completedTask );
                Next.React( nextEvent );
                _subscriber.Dispose();
                Next.OnDispose( _disposalSource );
            }
        }
    }
}
