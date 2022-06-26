using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

public readonly struct TaskSchedulerCapture
{
    private readonly TaskScheduler? _scheduler;
    private readonly bool _lazyCapture;

    public TaskSchedulerCapture(TaskScheduler scheduler)
    {
        _scheduler = scheduler;
        _lazyCapture = false;
    }

    public TaskSchedulerCapture(TaskSchedulerCaptureStrategy strategy)
    {
        switch ( strategy )
        {
            case TaskSchedulerCaptureStrategy.Current:
                _scheduler = GetCurrentScheduler();
                _lazyCapture = false;
                break;

            case TaskSchedulerCaptureStrategy.Lazy:
                _scheduler = null;
                _lazyCapture = true;
                break;

            default:
                _scheduler = null;
                _lazyCapture = false;
                break;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskScheduler GetCurrentScheduler()
    {
        return SynchronizationContext.Current is not null
            ? TaskScheduler.FromCurrentSynchronizationContext()
            : TaskScheduler.Current;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskScheduler FromSynchronizationContext(SynchronizationContext context)
    {
        using var contextSwitch = new SynchronizationContextSwitch( context );
        return TaskScheduler.FromCurrentSynchronizationContext();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TaskScheduler? TryGetScheduler()
    {
        return _lazyCapture ? GetCurrentScheduler() : _scheduler;
    }
}
