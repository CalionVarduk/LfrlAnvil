// Copyright 2026 Łukasz Furlepa
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Internal;

internal sealed class EnumerableMemory<T> : IReadOnlyList<T>
{
    private readonly ReadOnlyMemory<T> _source;

    internal EnumerableMemory(ReadOnlyMemory<T> source)
    {
        _source = source;
    }

    public int Count => _source.Length;
    public T this[int index] => _source.Span[index];

    [Pure]
    public IEnumerator<T> GetEnumerator()
    {
        for ( var i = 0; i < _source.Length; ++i )
            yield return _source.Span[i];
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
