using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct DependencyLocatorStore : IDisposable
{
    private readonly Dictionary<Type, object> _cachesByKeyType;

    private DependencyLocatorStore(
        KeyedDependencyResolversStore keyedResolversStore,
        DependencyResolversStore globalResolvers,
        DependencyScope scope)
    {
        KeyedResolversStore = keyedResolversStore;
        Global = new DependencyLocator( scope, globalResolvers );
        _cachesByKeyType = new Dictionary<Type, object>();
    }

    internal DependencyLocator Global { get; }
    internal KeyedDependencyResolversStore KeyedResolversStore { get; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        _cachesByKeyType.Clear();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorStore Create(
        KeyedDependencyResolversStore keyedResolversStore,
        DependencyResolversStore globalResolvers,
        DependencyScope scope)
    {
        return new DependencyLocatorStore( keyedResolversStore, globalResolvers, scope );
    }

    internal DependencyLocator<TKey> GetOrCreate<TKey>(TKey key)
        where TKey : notnull
    {
        KeyedCache<TKey>? cache = null;
        using ( ReadLockSlim.TryEnter( Global.InternalAttachedScope.Lock, out var entered ) )
        {
            if ( ! entered || Global.InternalAttachedScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( Global.InternalAttachedScope ) ) );

            if ( _cachesByKeyType.TryGetValue( typeof( TKey ), out var outCache ) )
            {
                cache = ReinterpretCast.To<KeyedCache<TKey>>( outCache );
                if ( cache.Locators.TryGetValue( key, out var locator ) )
                    return locator;
            }
        }

        using ( WriteLockSlim.TryEnter( Global.InternalAttachedScope.Lock, out var entered ) )
        {
            if ( ! entered || Global.InternalAttachedScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( Global.InternalAttachedScope ) ) );

            if ( cache is null )
            {
                ref var cacheRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _cachesByKeyType, typeof( TKey ), out var cacheExists )!;
                if ( cacheExists )
                    cache = ReinterpretCast.To<KeyedCache<TKey>>( cacheRef );
                else
                {
                    cache = new KeyedCache<TKey>();
                    cacheRef = cache;
                }
            }

            ref var locator = ref CollectionsMarshal.GetValueRefOrAddDefault( cache.Locators, key, out var exists )!;
            if ( ! exists )
            {
                var resolvers = KeyedResolversStore.GetResolversStore( key, Global.InternalAttachedScope.InternalContainer );
                locator = new DependencyLocator<TKey>( key, Global.InternalAttachedScope, resolvers );
            }

            return locator;
        }
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
