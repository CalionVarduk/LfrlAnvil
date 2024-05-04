using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Composites;

/// <summary>
/// Represents an event that is the result of a <see cref="Task{TResult}"/> completion.
/// </summary>
/// <typeparam name="TEvent"></typeparam>
public readonly struct FromTask<TEvent>
{
    /// <summary>
    /// Creates a new <see cref="FromTask{TEvent}"/> instance.
    /// </summary>
    /// <param name="task">Source <see cref="Task{TResult}"/>.</param>
    public FromTask(Task<TEvent> task)
    {
        Result = task.Status == TaskStatus.RanToCompletion ? task.Result : default;
        Exception = task.Exception;
        Status = task.Status;
    }

    /// <summary>
    /// Status of the source task.
    /// </summary>
    public TaskStatus Status { get; }

    /// <summary>
    /// Result of the source task.
    /// </summary>
    public TEvent? Result { get; }

    /// <summary>
    /// Exception of the source task.
    /// </summary>
    public AggregateException? Exception { get; }

    /// <summary>
    /// Specifies whether or not the source task has been cancelled.
    /// </summary>
    public bool IsCanceled => Status == TaskStatus.Canceled;

    /// <summary>
    /// Specifies whether or not the source task has faulted.
    /// </summary>
    public bool IsFaulted => Status == TaskStatus.Faulted;

    /// <summary>
    /// Specifies whether or not the source task ran to completion.
    /// </summary>
    public bool IsCompletedSuccessfully => Status == TaskStatus.RanToCompletion;

    /// <summary>
    /// Returns a string representation of this <see cref="FromTask{TEvent}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var statusText = $"[{Status}]";

        if ( IsCanceled )
            return statusText;

        return IsFaulted ? $"{statusText}: {Exception}" : $"{statusText}: {Result}";
    }
}
