using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public readonly struct WriteLockSlim : IDisposable
{
    private readonly ReaderWriterLockSlim? _lock;

    private WriteLockSlim(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static WriteLockSlim Enter(ReaderWriterLockSlim @lock)
    {
        @lock.EnterWriteLock();
        return new WriteLockSlim( @lock );
    }

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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock?.ExitWriteLock();
    }
}
