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
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Internal;

internal static class Buffers
{
    internal const int MinCapacity = 1 << 2;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static int GetCapacity(int minCapacity)
    {
        if ( minCapacity <= MinCapacity )
            minCapacity = MinCapacity;

        var result = BitOperations.RoundUpToPowerOf2( unchecked( ( uint )minCapacity ) );
        return result > int.MaxValue ? int.MaxValue : unchecked( ( int )result );
    }
}
