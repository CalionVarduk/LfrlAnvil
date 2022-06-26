using System;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class LambdaEventListener<TEvent> : EventListener<TEvent>
{
    private readonly Action<TEvent> _react;
    private readonly Action<DisposalSource>? _dispose;

    internal LambdaEventListener(Action<TEvent> react, Action<DisposalSource>? dispose)
    {
        _react = react;
        _dispose = dispose;
    }

    public override void React(TEvent @event)
    {
        _react( @event );
    }

    public override void OnDispose(DisposalSource source)
    {
        _dispose?.Invoke( source );
    }
}
