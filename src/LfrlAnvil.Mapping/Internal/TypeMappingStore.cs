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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping.Internal;

/// <summary>
/// Represents a container for delegates used for type mapping.
/// </summary>
public readonly struct TypeMappingStore
{
    private TypeMappingStore(Delegate fastDelegate, Delegate slowDelegate)
    {
        FastDelegate = fastDelegate;
        SlowDelegate = slowDelegate;
    }

    /// <summary>
    /// Fast delegate. Used when both source and destination types are known.
    /// </summary>
    public Delegate FastDelegate { get; }

    /// <summary>
    /// Slow delegate. Used when either source or destination type is unknown.
    /// </summary>
    public Delegate SlowDelegate { get; }

    /// <summary>
    /// Returns the <see cref="FastDelegate"/>.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><see cref="FastDelegate"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// When <see cref="FastDelegate"/> is not a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Func<TSource, ITypeMapper, TDestination> GetDelegate<TSource, TDestination>()
    {
        return ( Func<TSource, ITypeMapper, TDestination> )FastDelegate;
    }

    /// <summary>
    /// Returns the <see cref="SlowDelegate"/>.
    /// </summary>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><see cref="SlowDelegate"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// When <see cref="SlowDelegate"/> is not an <see cref="Object"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Func<object, ITypeMapper, TDestination> GetDelegate<TDestination>()
    {
        return ( Func<object, ITypeMapper, TDestination> )SlowDelegate;
    }

    /// <summary>
    /// Returns the <see cref="SlowDelegate"/>.
    /// </summary>
    /// <returns><see cref="SlowDelegate"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// When <see cref="SlowDelegate"/> is not an <see cref="Object"/> => <see cref="Object"/> mapping definition.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Func<object, ITypeMapper, object> GetDelegate()
    {
        return ( Func<object, ITypeMapper, object> )SlowDelegate;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static TypeMappingStore Create<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        Func<object, ITypeMapper, TDestination> slowMapping = (source, provider) => mapping( ( TSource )source, provider );
        return new TypeMappingStore( mapping, slowMapping );
    }
}
