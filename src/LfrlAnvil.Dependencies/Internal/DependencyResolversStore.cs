// Copyright 2024-2026 Łukasz Furlepa
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
using System.Runtime.InteropServices;
using System.Threading;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyResolversStore : IDisposable
{
    internal readonly ReaderWriterLockSlim Lock;
    internal readonly Dictionary<Type, DependencyResolver> Resolvers;
    internal readonly Dictionary<SharedGenericKey, DependencyResolver> SharedGenericResolvers;

    private DependencyResolversStore(Dictionary<Type, DependencyResolver> resolvers)
    {
        Lock = new ReaderWriterLockSlim( LockRecursionPolicy.SupportsRecursion );
        Resolvers = resolvers;
        SharedGenericResolvers = new Dictionary<SharedGenericKey, DependencyResolver>();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        Lock.DisposeGracefully();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyResolversStore Create(Dictionary<Type, DependencyResolver> resolvers)
    {
        return new DependencyResolversStore( resolvers );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver? TryGetResolver(Type type)
    {
        Assume.True( Lock.IsReadLockHeld || Lock.IsUpgradeableReadLockHeld );
        return Resolvers.TryGetValue( type, out var resolver ) ? resolver : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver GetOrAddResolver(Type type, DependencyResolver resolver)
    {
        Assume.True( Lock.IsWriteLockHeld );
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault( Resolvers, type, out var exists )!;
        if ( ! exists )
            current = resolver;

        return current;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetResolver(Type type, DependencyResolver resolver)
    {
        Assume.True( Lock.IsWriteLockHeld );
        Resolvers[type] = resolver;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver? TryGetSharedGenericResolver(Type openType, Type closedType, DependencyLifetime lifetime)
    {
        Assume.True( openType.IsGenericTypeDefinition );
        Assume.False( closedType.ContainsGenericParameters );
        return SharedGenericResolvers.TryGetValue( new SharedGenericKey( openType, closedType, lifetime ), out var value ) ? value : null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver GetOrAddSharedGenericResolver(
        Type openType,
        Type closedType,
        DependencyLifetime lifetime,
        DependencyResolver resolver)
    {
        Assume.True( openType.IsGenericTypeDefinition );
        Assume.False( closedType.ContainsGenericParameters );
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(
            SharedGenericResolvers,
            new SharedGenericKey( openType, closedType, lifetime ),
            out var exists )!;

        if ( ! exists )
            current = resolver;

        return current;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyLifetime? TryGetLifetime(Type type)
    {
        using ( ReadLockSlim.TryEnter( Lock, out var entered ) )
            return entered && Resolvers.TryGetValue( type, out var resolver ) ? resolver.Lifetime : null;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Type[] GetResolvableTypes()
    {
        using ( ReadLockSlim.TryEnter( Lock, out var entered ) )
        {
            if ( ! entered )
                return Type.EmptyTypes;

            Assume.ContainsAtLeast( Resolvers, 1 );
            var result = new Type[Resolvers.Count];
            Resolvers.Keys.CopyTo( result, 0 );
            return result;
        }
    }
}
