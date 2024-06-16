using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Async;

/// <summary>
/// A lightweight, disposable object representing an acquired semaphore entry.
/// </summary>
public readonly struct SemaphoreEntrySlim : IDisposable
{
    private readonly SemaphoreSlim? _semaphore;

    private SemaphoreEntrySlim(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    /// <summary>
    /// Specifies whether or not a semaphore has been entered.
    /// </summary>
    public bool Entered => _semaphore is not null;

    /// <summary>
    /// Enters a <see cref="SemaphoreSlim"/> instance by blocking the current thread.
    /// </summary>
    /// <param name="semaphore">A semaphore object to enter.</param>
    /// <returns>A disposable <see cref="SemaphoreEntrySlim"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">The <paramref name="semaphore"/> has been disposed.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SemaphoreEntrySlim Enter(SemaphoreSlim semaphore)
    {
        semaphore.Wait();
        return new SemaphoreEntrySlim( semaphore );
    }

    /// <summary>
    /// Attempts to enter a <see cref="SemaphoreSlim"/> instance by blocking the current thread.
    /// If the <paramref name="semaphore"/> has been disposed, then it will not be entered.
    /// </summary>
    /// <param name="semaphore">A semaphore object to enter.</param>
    /// <param name="entered">An <b>out</b> parameter set to <b>true</b> when semaphore has been entered, otherwise <b>false</b>.</param>
    /// <returns>
    /// A disposable <see cref="SemaphoreEntrySlim"/> instance.
    /// Semaphore will not be entered if <paramref name="semaphore"/> has been disposed.
    /// </returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SemaphoreEntrySlim TryEnter(SemaphoreSlim semaphore, out bool entered)
    {
        try
        {
            semaphore.Wait();
        }
        catch ( ObjectDisposedException )
        {
            entered = false;
            return default;
        }

        entered = true;
        return new SemaphoreEntrySlim( semaphore );
    }

    /// <summary>
    /// Enters a <see cref="SemaphoreSlim"/> instance asynchronously.
    /// </summary>
    /// <param name="semaphore">A semaphore object to enter.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task that returns a disposable <see cref="SemaphoreEntrySlim"/> instance.</returns>
    /// <exception cref="ObjectDisposedException">The <paramref name="semaphore"/> has been disposed.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SemaphoreEntrySlim> EnterAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync( cancellationToken ).ConfigureAwait( false );
        return new SemaphoreEntrySlim( semaphore );
    }

    /// <summary>
    /// Attempts to enter a <see cref="SemaphoreSlim"/> instance asynchronously.
    /// If the <paramref name="semaphore"/> has been disposed, then it will not be entered.
    /// </summary>
    /// <param name="semaphore">A semaphore object to enter.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    /// A task that returns a disposable <see cref="SemaphoreEntrySlim"/> instance.
    /// Semaphore will not be entered if <paramref name="semaphore"/> has been disposed.
    /// </returns>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SemaphoreEntrySlim> TryEnterAsync(SemaphoreSlim semaphore, CancellationToken cancellationToken = default)
    {
        try
        {
            await semaphore.WaitAsync( cancellationToken ).ConfigureAwait( false );
        }
        catch ( ObjectDisposedException )
        {
            return default;
        }

        return new SemaphoreEntrySlim( semaphore );
    }

    /// <inheritdoc />
    /// <exception cref="SemaphoreFullException">
    /// Attempt to release a semaphore has thrown an exception of this type.
    /// See <see cref="SemaphoreSlim.Release()"/> for more information.
    /// </exception>
    /// <remarks>Releases previously acquired semaphore entry.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        try
        {
            _semaphore?.Release();
        }
        catch ( ObjectDisposedException )
        {
            // NOTE: do nothing
        }
    }
}
