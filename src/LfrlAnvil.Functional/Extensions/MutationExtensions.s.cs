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

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="Mutation{T}"/> extension methods.
/// </summary>
public static class MutationExtensions
{
    /// <summary>
    /// Creates a new <see cref="Mutation{T}"/> instance.
    /// </summary>
    /// <param name="source">Source mutation.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Mutation{T}"/> instance with <see cref="Mutation{T}.OldValue"/> equal to
    /// nested <see cref="Mutation{T}.OldValue"/> and <see cref="Mutation{T}.Value"/> equal to nested <see cref="Mutation{T}.Value"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Mutation<T> Reduce<T>(this Mutation<Mutation<T>> source)
    {
        return new Mutation<T>( source.OldValue.OldValue, source.Value.Value );
    }
}
