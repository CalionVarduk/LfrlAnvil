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
/// Contains <see cref="TypeCast{TSource,TDestination}"/> extension methods.
/// </summary>
public static class TypeCastExtensions
{
    /// <summary>
    /// Creates a new <see cref="Maybe{T}"/> instance.
    /// </summary>
    /// <param name="source">Source type cast.</param>
    /// <typeparam name="TSource">Source object type.</typeparam>
    /// <typeparam name="TDestination">Destination object type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the result of <paramref name="source"/> or <see cref="Maybe{T}.None"/>
    /// when <see cref="TypeCast{TSource,TDestination}.IsValid"/> of <paramref name="source"/> is equal to <b>false</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<TDestination> ToMaybe<TSource, TDestination>(this TypeCast<TSource, TDestination> source)
        where TDestination : notnull
    {
        return source.IsValid ? new Maybe<TDestination>( source.Result ) : Maybe<TDestination>.None;
    }
}
