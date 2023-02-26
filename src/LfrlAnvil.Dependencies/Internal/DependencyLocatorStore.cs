using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyLocatorStore
{
    private readonly Dictionary<Type, object> _cachesByKeyType;

    private DependencyLocatorStore(
        KeyedDependencyResolversStore resolversStore,
        Dictionary<Type, DependencyResolver> globalResolvers,
        DependencyScope scope)
    {
        ResolversStore = resolversStore;
        Global = new DependencyLocator( scope, globalResolvers );
        _cachesByKeyType = new Dictionary<Type, object>();
    }

    internal DependencyLocator Global { get; }
    internal KeyedDependencyResolversStore ResolversStore { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorStore Create(
        KeyedDependencyResolversStore resolversStore,
        Dictionary<Type, DependencyResolver> globalResolvers,
        DependencyScope scope)
    {
        return new DependencyLocatorStore( resolversStore, globalResolvers, scope );
    }

    internal DependencyLocator<TKey> GetOrCreate<TKey>(TKey key)
        where TKey : notnull
    {
        ref var cacheRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _cachesByKeyType, typeof( TKey ), out var exists )!;
        if ( ! exists )
            cacheRef = new KeyedCache<TKey>();

        var cache = ReinterpretCast.To<KeyedCache<TKey>>( cacheRef );
        ref var locator = ref CollectionsMarshal.GetValueRefOrAddDefault( cache.Locators, key, out exists )!;
        if ( ! exists )
        {
            var resolvers = ResolversStore.GetResolvers( key );
            locator = new DependencyLocator<TKey>( key, Global.InternalAttachedScope, resolvers );
        }

        return locator;
    }

    internal void Clear()
    {
        _cachesByKeyType.Clear();
    }

    private sealed class KeyedCache<TKey>
        where TKey : notnull
    {
        internal readonly Dictionary<TKey, DependencyLocator<TKey>> Locators;

        internal KeyedCache()
        {
            Locators = new Dictionary<TKey, DependencyLocator<TKey>>();
        }
    }
}
