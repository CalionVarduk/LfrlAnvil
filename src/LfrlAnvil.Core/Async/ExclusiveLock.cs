using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

public readonly struct ExclusiveLock : IDisposable
{
    private readonly object? _sync;

    private ExclusiveLock(object sync)
    {
        _sync = sync;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ExclusiveLock Enter(object sync)
    {
        Monitor.Enter( sync );
        return new ExclusiveLock( sync );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( _sync is not null )
            Monitor.Exit( _sync );
    }
}
