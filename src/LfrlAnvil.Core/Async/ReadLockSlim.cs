using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public readonly struct ReadLockSlim : IDisposable
{
    private readonly ReaderWriterLockSlim? _lock;

    private ReadLockSlim(ReaderWriterLockSlim @lock)
    {
        _lock = @lock;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadLockSlim Enter(ReaderWriterLockSlim @lock)
    {
        @lock.EnterReadLock();
        return new ReadLockSlim( @lock );
    }

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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _lock?.ExitReadLock();
    }
}
