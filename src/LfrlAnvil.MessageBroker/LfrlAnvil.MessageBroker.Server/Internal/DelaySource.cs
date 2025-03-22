// Copyright 2025 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Async;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct DelaySource
{
    private ValueTaskDelaySource? _source;
    private readonly bool _owned;

    private DelaySource(ValueTaskDelaySource? source, bool owned)
    {
        _source = source;
        _owned = owned;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DelaySource External(ValueTaskDelaySource source)
    {
        return new DelaySource( source, owned: false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DelaySource Owned()
    {
        return new DelaySource( null, owned: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTaskDelaySource GetSource()
    {
        if ( _owned )
        {
            Assume.IsNull( _source );
            _source = ValueTaskDelaySource.Start();
        }

        Assume.IsNotNull( _source );
        return _source;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTaskDelaySource? DiscardOwnedSource()
    {
        var result = _source;
        _source = null;
        return _owned ? result : null;
    }
}
