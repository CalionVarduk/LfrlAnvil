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
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Dependencies.Internal;

internal sealed class DependencyKey : IInternalDependencyKey
{
    internal DependencyKey(Type type)
    {
        Type = type;
    }

    public Type Type { get; }
    public Type? KeyType => null;
    public object? Key => null;
    public bool IsKeyed => false;

    [Pure]
    public override string ToString()
    {
        return $"'{Type.GetDebugString()}'";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Type.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return IsEqualTo( obj as DependencyKey );
    }

    [Pure]
    public bool Equals(IDependencyKey? other)
    {
        return IsEqualTo( other as DependencyKey );
    }

    [Pure]
    public DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore)
    {
        return builderStore.Global.SharedImplementors.GetValueOrDefault( Type );
    }

    [Pure]
    public Dictionary<Type, DependencyResolver> GetTargetResolvers(
        Dictionary<Type, DependencyResolver> globalResolvers,
        KeyedDependencyResolversStore keyedResolversStore)
    {
        return globalResolvers;
    }

    [Pure]
    public IInternalDependencyKey WithType(Type type)
    {
        return new DependencyKey( type );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsEqualTo(DependencyKey? other)
    {
        return other is not null && Type == other.Type;
    }
}

internal sealed class DependencyKey<TKey> : IInternalDependencyKey, IDependencyKey<TKey>
    where TKey : notnull
{
    private static readonly IEqualityComparer<TKey> KeyComparer = EqualityComparer<TKey>.Default;

    internal DependencyKey(Type type, TKey key)
    {
        Type = type;
        Key = key;
    }

    public Type Type { get; }
    public TKey Key { get; }
    public Type KeyType => typeof( TKey );
    public bool IsKeyed => true;

    object IDependencyKey.Key => Key;

    [Pure]
    public override string ToString()
    {
        return $"'{Type.GetDebugString()}' [{nameof( Key )}: '{Key}']";
    }

    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine( Type, Key );
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return IsEqualTo( obj as DependencyKey<TKey> );
    }

    [Pure]
    public bool Equals(IDependencyKey? other)
    {
        return IsEqualTo( other as DependencyKey<TKey> );
    }

    [Pure]
    public DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore)
    {
        var locator = builderStore.GetKeyed( Key );
        return locator?.SharedImplementors.GetValueOrDefault( Type );
    }

    [Pure]
    public Dictionary<Type, DependencyResolver> GetTargetResolvers(
        Dictionary<Type, DependencyResolver> globalResolvers,
        KeyedDependencyResolversStore keyedResolversStore)
    {
        return keyedResolversStore.GetOrAddResolvers( Key );
    }

    [Pure]
    public IInternalDependencyKey WithType(Type type)
    {
        return new DependencyKey<TKey>( type, Key );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private bool IsEqualTo(DependencyKey<TKey>? other)
    {
        return other is not null && Type == other.Type && KeyComparer.Equals( Key, other.Key );
    }
}
