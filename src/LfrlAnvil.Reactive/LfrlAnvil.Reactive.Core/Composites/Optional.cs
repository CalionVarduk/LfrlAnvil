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

using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Composites;

internal readonly struct Optional<TEvent>
{
    internal static readonly Optional<TEvent> Empty = new Optional<TEvent>();

    internal Optional(TEvent @event)
    {
        Event = @event;
        HasValue = true;
    }

    internal readonly TEvent? Event;
    internal readonly bool HasValue;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void TryForward(IEventListener<TEvent> listener)
    {
        if ( HasValue )
            listener.React( Event! );
    }
}
