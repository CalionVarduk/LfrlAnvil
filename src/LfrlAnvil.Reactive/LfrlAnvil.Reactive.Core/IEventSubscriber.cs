using System;

namespace LfrlAnvil.Reactive;

public interface IEventSubscriber : IDisposable
{
    bool IsDisposed { get; }
}
