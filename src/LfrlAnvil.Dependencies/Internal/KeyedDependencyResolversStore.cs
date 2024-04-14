using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct KeyedDependencyResolversStore : IDisposable
{
    private readonly Dictionary<Type, IDisposable> _cachesByKeyType;
    private readonly IReadOnlyDictionary<Type, DependencyResolver> _defaultResolvers;

    private KeyedDependencyResolversStore(IReadOnlyDictionary<Type, DependencyResolver> defaultResolvers)
    {
        _defaultResolvers = defaultResolvers;
        _cachesByKeyType = new Dictionary<Type, IDisposable>();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        foreach ( var cache in _cachesByKeyType.Values )
            cache.Dispose();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static KeyedDependencyResolversStore Create(IReadOnlyDictionary<Type, DependencyResolver> defaultResolvers)
    {
        return new KeyedDependencyResolversStore( defaultResolvers );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Dictionary<Type, DependencyResolver> GetOrAddResolvers<TKey>(TKey key)
        where TKey : notnull
    {
        ref var cacheRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _cachesByKeyType, typeof( TKey ), out var exists )!;
        if ( ! exists )
            cacheRef = new Cache<TKey>();

        var cache = ReinterpretCast.To<Cache<TKey>>( cacheRef );
        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( cache.Resolvers, key, out exists );
        if ( ! exists )
            result = DependencyResolversStore.Create( new Dictionary<Type, DependencyResolver>( _defaultResolvers ) );

        return result.Resolvers;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolversStore GetResolversStore<TKey>(TKey key, DependencyContainer container)
        where TKey : notnull
    {
        Cache<TKey>? cache = null;
        using ( ReadLockSlim.TryEnter( container.InternalRootScope.Lock, out var entered ) )
        {
            if ( ! entered || container.InternalRootScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( container.InternalRootScope ) ) );

            if ( _cachesByKeyType.TryGetValue( typeof( TKey ), out var outCache ) )
            {
                cache = ReinterpretCast.To<Cache<TKey>>( outCache );
                if ( cache.Resolvers.TryGetValue( key, out var resolvers ) )
                    return resolvers;
            }
        }

        using ( WriteLockSlim.TryEnter( container.InternalRootScope.Lock, out var entered ) )
        {
            if ( ! entered || container.InternalRootScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( container.InternalRootScope ) ) );

            if ( cache is null )
            {
                ref var cacheRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _cachesByKeyType, typeof( TKey ), out var cacheExists )!;
                if ( cacheExists )
                    cache = ReinterpretCast.To<Cache<TKey>>( cacheRef );
                else
                {
                    cache = new Cache<TKey>();
                    cacheRef = cache;
                }
            }

            ref var resolvers = ref CollectionsMarshal.GetValueRefOrAddDefault( cache.Resolvers, key, out var exists );
            if ( ! exists )
                resolvers = DependencyResolversStore.Create( new Dictionary<Type, DependencyResolver>( _defaultResolvers ) );

            return resolvers;
        }
    }

    private sealed class Cache<TKey> : IDisposable
        where TKey : notnull
    {
        internal readonly Dictionary<TKey, DependencyResolversStore> Resolvers;

        internal Cache()
        {
            Resolvers = new Dictionary<TKey, DependencyResolversStore>();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Dispose()
        {
            foreach ( var store in Resolvers.Values )
                store.Dispose();
        }
    }
}
