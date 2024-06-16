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
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Defines methods to support the comparison of <see cref="ReadOnlyMemory{T}"/> objects for equality.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class MemoryElementWiseComparer<T> : IEqualityComparer<ReadOnlyMemory<T>>
{
    /// <inheritdoc />
    [Pure]
    public bool Equals(ReadOnlyMemory<T> x, ReadOnlyMemory<T> y)
    {
        if ( x.Length != y.Length )
            return false;

        var xSpan = x.Span;
        var ySpan = y.Span;

        for ( var i = 0; i < xSpan.Length; ++i )
        {
            if ( ! Generic<T>.AreEqual( xSpan[i], ySpan[i] ) )
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    [Pure]
    public int GetHashCode(ReadOnlyMemory<T> obj)
    {
        var result = Hash.Default;
        foreach ( var o in obj )
            result = result.Add( o );

        return result.Value;
    }
}
