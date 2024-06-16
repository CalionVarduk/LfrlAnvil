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

internal abstract class DecoratedEventSourceBase<TRootEvent, TEvent> : IEventStream<TEvent>
{
    protected DecoratedEventSourceBase(EventSource<TRootEvent> root)
    {
        Root = root;
    }

    public bool IsDisposed => Root.IsDisposed;
    protected EventSource<TRootEvent> Root { get; }

    public IEventSubscriber Listen(IEventListener<TEvent> listener)
    {
        return Listen( listener, Root.CreateSubscriber() );
    }

    [Pure]
    public abstract IEventStream<TNextEvent> Decorate<TNextEvent>(IEventListenerDecorator<TEvent, TNextEvent> decorator);

    internal abstract IEventSubscriber Listen(IEventListener<TEvent> listener, EventSubscriber<TRootEvent> subscriber);

    IEventSubscriber IEventStream.Listen(IEventListener listener)
    {
        return Listen( Argument.CastTo<IEventListener<TEvent>>( listener ) );
    }
}
