using System;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Reactive.Composites;

namespace LfrlAnvil.Reactive.Internal
{
    internal sealed class TaskEventSource<TEvent> : EventSource<FromTask<TEvent>>
    {
        private readonly Func<CancellationToken, Task<TEvent>> _taskFactory;
        private readonly TaskScheduler? _callbackScheduler;
        private readonly bool _captureListenerContext;

        internal TaskEventSource(
            Func<CancellationToken, Task<TEvent>> taskFactory,
            TaskScheduler callbackScheduler)
        {
            _taskFactory = taskFactory;
            _callbackScheduler = callbackScheduler;
            _captureListenerContext = false;
        }

        internal TaskEventSource(
            Func<CancellationToken, Task<TEvent>> taskFactory,
            TaskEventSourceContextCapture contextCapture)
        {
            _taskFactory = taskFactory;

            switch ( contextCapture )
            {
                case TaskEventSourceContextCapture.Current:
                    _callbackScheduler = CapturedTaskScheduler.GetCurrent();
                    _captureListenerContext = false;
                    break;

                case TaskEventSourceContextCapture.FromListener:
                    _callbackScheduler = null;
                    _captureListenerContext = true;
                    break;

                default:
                    _callbackScheduler = null;
                    _captureListenerContext = false;
                    break;
            }
        }

        protected override IEventListener<FromTask<TEvent>> OverrideListener(
            IEventSubscriber subscriber,
            IEventListener<FromTask<TEvent>> listener)
        {
            if ( IsDisposed )
                return listener;

            var callbackScheduler = _captureListenerContext ? CapturedTaskScheduler.GetCurrent() : _callbackScheduler;
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
