using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight, disposable object representing an acquired upgradeable read lock.
/// </summary>
public readonly struct UpgradeableReadLockSlim : IDisposable
{
    private readonly ReaderWriterLockSlim? _lock;

    private UpgradeableReadLockSlim(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
    }

    /// <summary>
    /// Acquires an upgradeable read lock on the provided <see cref="ReaderWriterLockSlim"/> instance.
    /// </summary>
    /// <param name="lock">A reader/writer lock object on which to acquire the upgradeable read lock.</param>
    /// <returns>A disposable <see cref="UpgradeableReadLockSlim"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">The <paramref name="lock"/> has been disposed.</exception>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire an upgradeable read lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterUpgradeableReadLock()"/> for more information.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static UpgradeableReadLockSlim Enter(ReaderWriterLockSlim @lock)
    {
        @lock.EnterUpgradeableReadLock();
        return new UpgradeableReadLockSlim( @lock );
    }

    /// <summary>
    /// Attempts to acquire an upgradeable read lock on the provided <see cref="ReaderWriterLockSlim"/> instance.
    /// If the <paramref name="lock"/> has been disposed, then the upgradeable read lock will not be acquired.
    /// </summary>
    /// <param name="lock">A reader/writer lock object on which to acquire the upgradeable read lock.</param>
    /// <param name="entered">
    /// An <b>out</b> parameter set to <b>true</b> when upgradeable read lock has been acquired, otherwise <b>false</b>.
    /// </param>
    /// <returns>
    /// A disposable <see cref="UpgradeableReadLockSlim"/> instance.
    /// Upgradeable read lock will not be acquired if <paramref name="lock"/> has been disposed.
    /// </returns>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire an upgradeable read lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterUpgradeableReadLock()"/> for more information.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static UpgradeableReadLockSlim TryEnter(ReaderWriterLockSlim @lock, out bool entered)
    {
        try
        {
            @lock.EnterUpgradeableReadLock();
        }
        catch ( ObjectDisposedException )
        {
            entered = false;
            return default;
        }

        entered = true;
        return new UpgradeableReadLockSlim( @lock );
    }

    /// <summary>
    /// Attempts to upgrade acquired read lock to write lock.
    /// </summary>
    /// <returns>
    /// A disposable <see cref="WriteLockSlim"/> instance.
    /// Write lock will not be acquired if this upgradeable read lock has not been acquired beforehand.
    /// </returns>
    /// <exception cref="ObjectDisposedException">The underlying <see cref="ReaderWriterLockSlim"/> instance has been disposed.</exception>
    /// <exception cref="LockRecursionException">
    /// Attempt to acquire a write lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.EnterWriteLock()"/> for more information.
    /// </exception>
    /// <remarks>
    /// This method assumes that the current thread holds an upgradeable read lock. See <see cref="Assume"/> for more information.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public WriteLockSlim Upgrade()
    {
        if ( _lock is null )
            return default;

        Assume.True( _lock.IsUpgradeableReadLockHeld );
        return WriteLockSlim.Enter( _lock );
    }

    /// <inheritdoc />
    /// <exception cref="SynchronizationLockException">
    /// Attempt to release an upgradeable read lock has thrown an exception of this type.
    /// See <see cref="ReaderWriterLockSlim.ExitUpgradeableReadLock()"/> for more information.
    /// </exception>
    /// <remarks>Releases previously acquired upgradeable read lock.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock?.ExitUpgradeableReadLock();
    }
}
