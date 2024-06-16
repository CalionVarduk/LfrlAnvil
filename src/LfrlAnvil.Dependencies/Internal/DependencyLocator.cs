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

namespace LfrlAnvil.Dependencies.Internal;

internal class DependencyLocator : IDependencyLocator
{
    internal DependencyLocator(DependencyScope attachedScope, DependencyResolversStore resolvers)
    {
        InternalAttachedScope = attachedScope;
        Resolvers = resolvers;
    }

    public IDependencyScope AttachedScope => InternalAttachedScope;
    internal DependencyScope InternalAttachedScope { get; }
    internal DependencyResolversStore Resolvers { get; }

    Type? IDependencyLocator.KeyType => null;
    object? IDependencyLocator.Key => null;
    bool IDependencyLocator.IsKeyed => false;

    public object? TryResolveUnsafe(Type type)
    {
        return InternalAttachedScope.InternalContainer.TryResolveDependency( this, type );
    }

    [Pure]
    public DependencyLifetime? TryGetLifetime(Type type)
    {
        return Resolvers.TryGetLifetime( type );
    }

    [Pure]
    public Type[] GetResolvableTypes()
    {
        return Resolvers.GetResolvableTypes();
    }
}

internal sealed class DependencyLocator<TKey> : DependencyLocator, IDependencyLocator<TKey>
    where TKey : notnull
{
    internal DependencyLocator(TKey key, DependencyScope attachedScope, DependencyResolversStore resolvers)
        : base( attachedScope, resolvers )
    {
        Key = key;
    }

    public TKey Key { get; }

    Type IDependencyLocator.KeyType => typeof( TKey );
    object IDependencyLocator.Key => Key;
    bool IDependencyLocator.IsKeyed => true;
}
