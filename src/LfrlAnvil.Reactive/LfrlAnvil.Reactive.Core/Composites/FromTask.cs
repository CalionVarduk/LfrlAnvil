using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LfrlAnvil.Reactive.Composites;

public readonly struct FromTask<TEvent>
{
    public FromTask(Task<TEvent> task)
    {
        Result = task.Status == TaskStatus.RanToCompletion ? task.Result : default;
        Exception = task.Exception;
        Status = task.Status;
    }

    public TaskStatus Status { get; }
    public TEvent? Result { get; }
    public AggregateException? Exception { get; }
    public bool IsCanceled => Status == TaskStatus.Canceled;
    public bool IsFaulted => Status == TaskStatus.Faulted;
    public bool IsCompletedSuccessfully => Status == TaskStatus.RanToCompletion;

    [Pure]
    public override string ToString()
    {
        var statusText = $"[{Status}]";

        if ( IsCanceled )
            return statusText;

        return IsFaulted ? $"{statusText}: {Exception}" : $"{statusText}: {Result}";
    }
}