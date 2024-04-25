using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight, disposable object representing an acquired write lock.
/// </summary>
public readonly struct WriteLockSlim : IDisposable
{
    private readonly ReaderWriterLockSlim? _lock;

    private WriteLockSlim(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
    }

    /// <summary>
    /// Acquires a write lock on the provided <see cref="ReaderWriterLockSlim"/> instance.
    /// </summary>
    /// <param name="lock">A reader/writer lock object on which to acquire the write lock.</param>
    /// <returns>A disposable <see cref="WriteLockSlim"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">The <paramref name="lock"/> has been disposed.</exception>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire a write lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterWriteLock()"/> for more information.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WriteLockSlim Enter(ReaderWriterLockSlim @lock)
    {
        @lock.EnterWriteLock();
        return new WriteLockSlim( @lock );
    }

    /// <summary>
    /// Attempts to acquire a write lock on the provided <see cref="ReaderWriterLockSlim"/> instance.
    /// If the <paramref name="lock"/> has been disposed, then the write lock will not be acquired.
    /// </summary>
    /// <param name="lock">A reader/writer lock object on which to acquire the write lock.</param>
    /// <param name="entered">An <b>out</b> parameter set to <b>true</b> when write lock has been acquired, otherwise <b>false</b>.</param>
    /// <returns>
    /// A disposable <see cref="WriteLockSlim"/> instance. Write lock will not be acquired if <paramref name="lock"/> has been disposed.
    /// </returns>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire a write lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterWriteLock()"/> for more information.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WriteLockSlim TryEnter(ReaderWriterLockSlim @lock, out bool entered)
    {
        try
        {
            @lock.EnterWriteLock();
        }
        catch ( ObjectDisposedException )
        {
            entered = false;
            return default;
        }

        entered = true;
        return new WriteLockSlim( @lock );
    }

    /// <inheritdoc />
    /// <exception cref="SynchronizationLockException">
    /// Attempt to release a write lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.ExitWriteLock()"/> for more information.
    /// </exception>
    /// <remarks>Releases previously acquired write lock.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock?.ExitWriteLock();
    }
}
