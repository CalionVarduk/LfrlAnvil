using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public struct DisposableLock : IDisposable
{
    private readonly object _sync;
    private bool _isTaken;

    public DisposableLock(object sync)
    {
        _sync = sync;
        _isTaken = false;
        Monitor.Enter( _sync, ref _isTaken );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( ! _isTaken )
            return;

        Monitor.Exit( _sync );
        _isTaken = false;
    }
}
