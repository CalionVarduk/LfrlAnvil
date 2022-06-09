using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Reactive.Queues
{
    public interface IMutableEventQueue<TEvent, TPoint, TPointDelta> : IEventQueue<TEvent, TPoint, TPointDelta>
    {
        void Move(TPointDelta delta);
        void Clear();
        TEvent Dequeue();
        bool TryDequeue([MaybeNullWhen( false )] out TEvent result);
    }
}
