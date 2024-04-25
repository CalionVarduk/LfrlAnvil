using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight, disposable object representing an acquired monitor lock.
/// </summary>
public readonly struct ExclusiveLock : IDisposable
{
    private readonly object? _sync;

    private ExclusiveLock(object sync)
    {
        _sync = sync;
    }

    /// <summary>
    /// Acquires an exclusive lock and creates a new <see cref="ExclusiveLock"/>.
    /// </summary>
    /// <param name="sync">An object on which to acquire the monitor lock.</param>
    /// <returns>A disposable <see cref="ExclusiveLock"/> instance.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ExclusiveLock Enter(object sync)
    {
        Monitor.Enter( sync );
        return new ExclusiveLock( sync );
    }

    /// <inheritdoc />
    /// <remarks>Releases previously acquired monitor lock.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( _sync is not null )
            Monitor.Exit( _sync );
    }
}
