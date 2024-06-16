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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Internal;

internal sealed class GrowingBuffer<T>
{
    internal const int BaseCapacity = 15;

    private T[] _data;
    private int _count;

    internal GrowingBuffer()
    {
        _data = new T[BaseCapacity];
        _count = 0;
    }

    public void Add(T item)
    {
        if ( _count == _data.Length )
            Array.Resize( ref _data, (_data.Length << 1) + 1 );

        _data[_count++] = item;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void RemoveAll()
    {
        Array.Clear( _data, 0, _count );
        _count = 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Clear()
    {
        _data = Array.Empty<T>();
        _count = 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlyMemory<T> AsMemory()
    {
        return _data.AsMemory( 0, _count );
    }
}
