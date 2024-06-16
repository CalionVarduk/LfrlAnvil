// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.Contracts;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class DecoratedEventSource<TRootEvent, TEvent> : DecoratedEventSourceBase<TRootEvent, TEvent>
{
    private readonly IEventListenerDecorator<TRootEvent, TEvent> _decorator;

    internal DecoratedEventSource(EventSource<TRootEvent> root, IEventListenerDecorator<TRootEvent, TEvent> decorator)
        : base( root )
    {
        _decorator = decorator;
    }

    [Pure]
    public override IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new DecoratedEventSource<TRootEvent, TEvent, TNextEvent>( Root, this, decorator );
    }

    internal override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber)
    {
        var sourceListener = _decorator.Decorate( listener, subscriber );
        return Root.Listen( sourceListener, subscriber );
    }
}

internal sealed class DecoratedEventSource<TRootEvent, TSourceEvent, TEvent> : DecoratedEventSourceBase<TRootEvent, TEvent>
{
    private readonly DecoratedEventSourceBase<TRootEvent, TSourceEvent> _base;
    private readonly IEventListenerDecorator<TSourceEvent, TEvent> _decorator;

    internal DecoratedEventSource(
        EventSource<TRootEvent> root,
        DecoratedEventSourceBase<TRootEvent, TSourceEvent> @base,
        IEventListenerDecorator<TSourceEvent, TEvent> decorator)
        : base( root )
    {
        _base = @base;
        _decorator = decorator;
    }

    [Pure]
    public override IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator)
    {
        return new DecoratedEventSource<TRootEvent, TEvent, TNextEvent>( Root, this, decorator );
    }

    internal override IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber)
    {
        var sourceListener = _decorator.Decorate( listener, subscriber );
        return _base.Listen( sourceListener, subscriber );
    }
}
