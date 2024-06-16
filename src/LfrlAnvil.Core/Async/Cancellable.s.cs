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
using System.Runtime.CompilerServices;
using System.Threading;

namespace LfrlAnvil.Async;

/// <summary>
/// Creates instances of <see cref="Cancellable{T}"/> type.
/// </summary>
public static class Cancellable
{
    /// <summary>
    /// Creates a <see cref="Cancellable{T}"/> instance.
    /// </summary>
    /// <param name="value">Value to assign.</param>
    /// <param name="token"><see cref="CancellationToken"/> to assign.</param>
    /// <typeparam name="T">Value's type.</typeparam>
    /// <returns>A <see cref="Cancellable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Cancellable<T> Create<T>(T value, CancellationToken token)
    {
        return new Cancellable<T>( value, token );
    }
}
