namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents the state of <see cref="ReactiveTimer"/>.
/// </summary>
public enum ReactiveTimerState
{
    /// <summary>
    /// Specifies that the timer is currently not running and not being stopped.
    /// </summary>
    Idle = 0,

    /// <summary>
    /// Specifies that the timer is currently running.
    /// </summary>
    Running = 1,

    /// <summary>
    /// Specifies that the timer is currently in the process of being stopped.
    /// </summary>
    Stopping = 2
}
