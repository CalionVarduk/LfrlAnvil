using System;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// A disposable current <see cref="SynchronizationContext"/> switch object that reverts the change once it gets disposed.
/// </summary>
public readonly struct SynchronizationContextSwitch : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="SynchronizationContextSwitch"/> instance.
    /// </summary>
    /// <param name="context">An optional <see cref="SynchronizationContext"/> instance to temporarily set as the current context.</param>
    public SynchronizationContextSwitch(SynchronizationContext? context)
    {
        PreviousContext = SynchronizationContext.Current;
        Context = context;
        SynchronizationContext.SetSynchronizationContext( Context );
    }

    /// <summary>
    /// A <see cref="SynchronizationContext"/> instance that was active before this switch has been created.
    /// This context will be set as current once the <see cref="Dispose()"/> method of this switch gets invoked.
    /// </summary>
    public SynchronizationContext? PreviousContext { get; }
    /// <summary>
    /// A <see cref="SynchronizationContext"/> instance activated by this switch.
    /// </summary>
    public SynchronizationContext? Context { get; }

    /// <inheritdoc />
    /// <remarks>
    /// Sets <see cref="PreviousContext"/> as the current synchronization context.
    /// </remarks>
    public void Dispose()
    {
        SynchronizationContext.SetSynchronizationContext( PreviousContext );
    }
}
