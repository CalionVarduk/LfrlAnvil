namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents the state of <see cref="ReactiveScheduler{TKey}"/>.
/// </summary>
public enum ReactiveSchedulerState
{
    /// <summary>
    /// Specifies that the scheduler has not been started yet.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Specifies that the scheduler is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Specifies that the scheduler is currently in the process of being disposed.
    /// </summary>
    Stopping = 2,

    /// <summary>
    /// Specifies that the scheduler has been disposed.
    /// </summary>
    Disposed = 3
}
