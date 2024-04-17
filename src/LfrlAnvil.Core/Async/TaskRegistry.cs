using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

public sealed class TaskRegistry : IDisposable, IAsyncDisposable
{
    private readonly Dictionary<int, Task> _tasks = new Dictionary<int, Task>();
    private readonly Action<Task> _taskContinuation;
    private InterlockedBoolean _isDisposed = new InterlockedBoolean( false );

    public TaskRegistry()
    {
        _taskContinuation = t =>
        {
            using ( ExclusiveLock.Enter( _tasks ) )
                _tasks.Remove( t.Id );
        };
    }

    public int Count
    {
        get
        {
            using ( ExclusiveLock.Enter( _tasks ) )
                return _tasks.Count;
        }
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        if ( ! _isDisposed.WriteTrue() )
            return;

        var task = WaitForAll();
        using ( ExclusiveLock.Enter( _tasks ) )
            _tasks.Clear();

        try
        {
            await task;
        }
        catch ( TaskCanceledException )
        {
            // NOTE:
            // ignore task cancellations
        }
    }

    public void Add(Task task)
    {
        using ( ExclusiveLock.Enter( _tasks ) )
        {
            if ( _isDisposed.Value )
                ExceptionThrower.Throw( new ObjectDisposedException( null, ExceptionResources.TaskRegistryIsDisposed ) );

            if ( task.IsCompleted )
                return;

            Ensure.NotEquals( task.Status, TaskStatus.Created, EqualityComparer<TaskStatus>.Default );

            _tasks.Add( task.Id, task );
            task.ContinueWith( _taskContinuation, TaskContinuationOptions.ExecuteSynchronously );
        }
    }

    [Pure]
    public bool Contains(Task task)
    {
        using ( ExclusiveLock.Enter( _tasks ) )
            return _tasks.ContainsKey( task.Id );
    }

    [Pure]
    public Task WaitForAll()
    {
        using ( ExclusiveLock.Enter( _tasks ) )
            return Task.WhenAll( _tasks.Values.ToArray() );
    }
}
