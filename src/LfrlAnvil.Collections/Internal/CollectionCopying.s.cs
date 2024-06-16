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
using System.Collections.Generic;

namespace LfrlAnvil.Collections.Internal;

internal static class CollectionCopying
{
    internal static void CopyTo<T>(ICollection<T> source, T[] array, int arrayIndex)
    {
        var count = Math.Min( source.Count, checked( array.Length - arrayIndex ) );
        var maxArrayIndex = checked( arrayIndex + count - 1 );

        if ( maxArrayIndex < 0 )
            return;

        using var enumerator = source.GetEnumerator();
        var index = arrayIndex;

        while ( index < 0 && enumerator.MoveNext() )
            ++index;

        while ( enumerator.MoveNext() && index <= maxArrayIndex )
            array[index++] = enumerator.Current;
    }
}
