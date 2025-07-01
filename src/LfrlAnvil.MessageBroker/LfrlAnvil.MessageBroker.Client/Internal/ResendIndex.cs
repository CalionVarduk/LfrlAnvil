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

namespace LfrlAnvil.MessageBroker.Client.Internal;

internal readonly struct ResendIndex
{
    private const uint ActiveMask = 1U << 31;
    internal readonly uint Data;

    internal ResendIndex(uint data)
    {
        Data = data;
    }

    internal int Value => unchecked( ( int )Data & int.MaxValue );
    internal bool IsActive => (Data & ActiveMask) != 0;

    [Pure]
    public override string ToString()
    {
        return $"{Value}{(IsActive ? " (active)" : string.Empty)}";
    }
}
