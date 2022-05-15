using System;

namespace LfrlAnvil.Reactive.Events
{
    public interface IEventSubscriber : IDisposable
    {
        bool IsDisposed { get; }
    }
}
