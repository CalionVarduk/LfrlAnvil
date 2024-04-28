namespace LfrlAnvil.Async;

/// <summary>
/// Represents a strategy for capturing a task scheduler.
/// </summary>
public enum TaskSchedulerCaptureStrategy : byte
{
    /// <summary>
    /// Specifies that the task scheduler should not be captured at all.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies that the current task scheduler should be captured.
    /// </summary>
    Current = 1,

    /// <summary>
    /// Specifies that the task scheduler should not be captured but should be returned on demand.
    /// </summary>
    Lazy = 2
}
