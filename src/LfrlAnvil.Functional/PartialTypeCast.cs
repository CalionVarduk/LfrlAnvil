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

namespace LfrlAnvil.Functional;

/// <summary>
/// An intermediate object used for creating <see cref="TypeCast{TSource,TDestination}"/> instances.
/// </summary>
/// <typeparam name="TSource">Source object type.</typeparam>
public readonly struct PartialTypeCast<TSource>
{
    /// <summary>
    /// Underlying source object.
    /// </summary>
    public readonly TSource Value;

    internal PartialTypeCast(TSource value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="TypeCast{TSource,TDestination}"/> instance by casting the source <see cref="Value"/>
    /// to the provided <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>New <see cref="TypeCast{TSource,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeCast<TSource, TDestination> To<TDestination>()
    {
        return new TypeCast<TSource, TDestination>( Value );
    }
}
