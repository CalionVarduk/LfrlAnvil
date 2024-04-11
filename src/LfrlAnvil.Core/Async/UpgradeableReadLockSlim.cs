using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public readonly struct UpgradeableReadLockSlim : IDisposable
{
    private readonly ReaderWriterLockSlim? _lock;

    private UpgradeableReadLockSlim(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static UpgradeableReadLockSlim Enter(ReaderWriterLockSlim @lock)
    {
        @lock.EnterUpgradeableReadLock();
        return new UpgradeableReadLockSlim( @lock );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public WriteLockSlim Upgrade()
    {
        if ( _lock is null )
            return default;

        Assume.True( _lock.IsUpgradeableReadLockHeld );
        return WriteLockSlim.Enter( _lock );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock?.ExitUpgradeableReadLock();
    }
}
