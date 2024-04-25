using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Async;

/// <summary>
/// Represents an object useful for storing in-progress <see cref="Task"/> instances.
/// </summary>
public sealed class TaskRegistry : IDisposable, IAsyncDisposable
{
    private readonly Dictionary<int, Task> _tasks = new Dictionary<int, Task>();
    private readonly Action<Task> _taskContinuation;
    private InterlockedBoolean _isDisposed = new InterlockedBoolean( false );

    /// <summary>
    /// Creates a new empty <see cref="TaskRegistry"/> instance.
    /// </summary>
    public TaskRegistry()
    {
        _taskContinuation = t =>
        {
            using ( ExclusiveLock.Enter( _tasks ) )
                _tasks.Remove( t.Id );
        };
    }

    /// <summary>
    /// Returns the current amount of stored in-progress <see cref="Task"/> instances.
    /// </summary>
    public int Count
    {
        get
        {
            using ( ExclusiveLock.Enter( _tasks ) )
                return _tasks.Count;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Marks this registry as disposed and waits for all stored in-progress <see cref="Task"/> instances to complete.
    /// </remarks>
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Marks this registry as disposed and returns a <see cref="ValueTask"/>
    /// that waits for all stored in-progress <see cref="Task"/> instances to complete.
    /// </remarks>
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

    /// <summary>
    /// Registers provided <paramref name="task"/> in this registry and stores it until it gets completed.
    /// Does nothing when <paramref name="task"/> is already completed.
    /// </summary>
    /// <param name="task"><see cref="Task"/> to register.</param>
    /// <exception cref="ObjectDisposedException">When this registry has been disposed.</exception>
    /// <exception cref="ArgumentException">When <see cref="TaskStatus"/> of <paramref name="task"/> is equal to <b>Created</b>.</exception>
    /// <remarks>
    /// Task continuation that is responsible for removing provided <paramref name="task"/> from this registry once it gets completed
    /// is created with the <see cref="TaskContinuationOptions.ExecuteSynchronously"/> option.
    /// </remarks>
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

    /// <summary>
    /// Checks if the provided <paramref name="task"/> is currently stored by this registry.
    /// </summary>
    /// <param name="task">Task to check for.</param>
    /// <returns><b>true</b> when <paramref name="task"/> is currently stored by this registry, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(Task task)
    {
        using ( ExclusiveLock.Enter( _tasks ) )
            return _tasks.ContainsKey( task.Id );
    }

    /// <summary>
    /// Creates a new <see cref="Task"/> that allows to wait for completion of all currently stored tasks in this registry.
    /// </summary>
    /// <returns><see cref="Task"/> instance that allows to wait for completion of all currently stored tasks in this registry.</returns>
    /// <remarks>
    /// Tasks added to this registry after this method invocation will not be dynamically appended to the returned task.
    /// See <see cref="Task.WhenAll(Task[])"/> for more information.
    /// </remarks>
    [Pure]
    public Task WaitForAll()
    {
        using ( ExclusiveLock.Enter( _tasks ) )
            return Task.WhenAll( _tasks.Values.ToArray() );
    }
}
