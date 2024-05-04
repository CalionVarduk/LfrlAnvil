using System;

namespace LfrlAnvil.Reactive;

/// <summary>
/// Represents a disposable event subscriber.
/// </summary>
public interface IEventSubscriber : IDisposable
{
    /// <summary>
    /// Specifies whether or not this event subscriber has been disposed.
    /// </summary>
    bool IsDisposed { get; }
}
