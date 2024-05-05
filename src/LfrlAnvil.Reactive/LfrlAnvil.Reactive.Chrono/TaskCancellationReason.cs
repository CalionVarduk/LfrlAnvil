using System.Threading;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Represents a reason for reactive task cancellation.
/// </summary>
public enum TaskCancellationReason : byte
{
    /// <summary>
    /// Specifies that a task has been cancelled due to <see cref="CancellationToken"/>.
    /// </summary>
    CancellationRequested = 0,

    /// <summary>
    /// Specifies that a task has been cancelled due to reached maximum task invocation queue size limit.
    /// </summary>
    MaxQueueSizeLimit = 1,

    /// <summary>
    /// Specifies that a task has been cancelled due to its definition being disposed.
    /// </summary>
    TaskDisposed = 2
}
