using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight, disposable object representing an acquired read lock.
/// </summary>
public readonly struct ReadLockSlim : IDisposable
{
    private readonly ReaderWriterLockSlim? _lock;

    private ReadLockSlim(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
    }

    /// <summary>
    /// Acquires a read lock on the provided <see cref="ReaderWriterLockSlim"/> instance.
    /// </summary>
    /// <param name="lock">A reader/writer lock object on which to acquire the read lock.</param>
    /// <returns>A disposable <see cref="ReadLockSlim"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">The <paramref name="lock"/> has been disposed.</exception>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire a read lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterReadLock()"/> for more information.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadLockSlim Enter(ReaderWriterLockSlim @lock)
    {
        @lock.EnterReadLock();
        return new ReadLockSlim( @lock );
    }

    /// <summary>
    /// Attempts to acquire a read lock on the provided <see cref="ReaderWriterLockSlim"/> instance.
    /// If the <paramref name="lock"/> has been disposed, then the read lock will not be acquired.
    /// </summary>
    /// <param name="lock">A reader/writer lock object on which to acquire the read lock.</param>
    /// <param name="entered">An <b>out</b> parameter set to <b>true</b> when read lock has been acquired, otherwise <b>false</b>.</param>
    /// <returns>
    /// A disposable <see cref="ReadLockSlim"/> instance. Read lock will not be acquired if <paramref name="lock"/> has been disposed.
    /// </returns>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire a read lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterReadLock()"/> for more information.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadLockSlim TryEnter(ReaderWriterLockSlim @lock, out bool entered)
    {
        try
        {
            @lock.EnterReadLock();
        }
        catch ( ObjectDisposedException )
        {
            entered = false;
            return default;
        }

        entered = true;
        return new ReadLockSlim( @lock );
    }

    /// <inheritdoc />
    /// <exception cref="SynchronizationLockException">
    /// Attempt to release a read lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.ExitReadLock()"/> for more information.
    /// </exception>
    /// <remarks>Releases previously acquired read lock.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock?.ExitReadLock();
    }
}
