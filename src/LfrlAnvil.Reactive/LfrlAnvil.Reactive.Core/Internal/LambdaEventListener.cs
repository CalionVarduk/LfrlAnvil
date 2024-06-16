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
