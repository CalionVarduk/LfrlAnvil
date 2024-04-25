using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight object capable of extracting <see cref="TaskScheduler"/> instances from the current <see cref="SynchronizationContext"/>
/// and capturing them.
/// </summary>
public readonly struct TaskSchedulerCapture
{
    private readonly TaskScheduler? _scheduler;
    private readonly bool _lazyCapture;

    /// <summary>
    /// Creates a new <see cref="TaskSchedulerCapture"/> instance with an explicit <see cref="TaskScheduler"/>.
    /// </summary>
    /// <param name="scheduler">An explicit <see cref="TaskScheduler"/> instance to capture.</param>
    public TaskSchedulerCapture(TaskScheduler? scheduler)
    {
        _scheduler = scheduler;
        _lazyCapture = false;
    }

    /// <summary>
    /// Creates a new <see cref="TaskSchedulerCapture"/> instance based on the provided <paramref name="strategy"/>.
    /// </summary>
    /// <param name="strategy">An option that defines the <see cref="TaskSchedulerCapture"/> instance's behavior.</param>
    /// <remarks>See <see cref="TaskSchedulerCaptureStrategy"/> for available strategies.</remarks>
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

    /// <summary>
    /// Returns the current <see cref="TaskScheduler"/> instance.
    /// </summary>
    /// <returns>Current <see cref="TaskScheduler"/> instance.</returns>
    /// <remarks>
    /// When <see cref="SynchronizationContext.Current"/> is not null,
    /// then returns the result of <see cref="TaskScheduler.FromCurrentSynchronizationContext()"/> invocation,
    /// otherwise returns <see cref="TaskScheduler"/>.<see cref="TaskScheduler.Current"/>.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskScheduler GetCurrentScheduler()
    {
        return SynchronizationContext.Current is not null
            ? TaskScheduler.FromCurrentSynchronizationContext()
            : TaskScheduler.Current;
    }

    /// <summary>
    /// Returns <see cref="TaskScheduler"/> instance associated with the provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">
    /// <see cref="SynchronizationContext"/> instance from which to get the <see cref="TaskScheduler"/> instance.
    /// </param>
    /// <returns><see cref="TaskScheduler"/> instance associated with the provided <paramref name="context"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TaskScheduler FromSynchronizationContext(SynchronizationContext context)
    {
        using var contextSwitch = new SynchronizationContextSwitch( context );
        return TaskScheduler.FromCurrentSynchronizationContext();
    }

    /// <summary>
    /// Returns the captured <see cref="TaskScheduler"/> instance or the current <see cref="TaskScheduler"/>
    /// if this capture has been created with the <see cref="TaskSchedulerCaptureStrategy.Lazy"/> option.
    /// </summary>
    /// <returns><see cref="TaskScheduler"/> associated with this capture.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TaskScheduler? TryGetScheduler()
    {
        return _lazyCapture ? GetCurrentScheduler() : _scheduler;
    }
}
