using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct KeyedDependencyResolversStore
{
    private readonly Dictionary<Type, object> _cachesByKeyType;
    private readonly IReadOnlyDictionary<Type, DependencyResolver> _defaultResolvers;

    private KeyedDependencyResolversStore(IReadOnlyDictionary<Type, DependencyResolver> defaultResolvers)
    {
        _defaultResolvers = defaultResolvers;
        _cachesByKeyType = new Dictionary<Type, object>();
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
        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( cache.Resolvers, key, out exists )!;
        if ( ! exists )
            result = new Dictionary<Type, DependencyResolver>( _defaultResolvers );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Dictionary<Type, DependencyResolver> GetResolvers<TKey>(TKey key)
        where TKey : notnull
    {
        if ( ! _cachesByKeyType.TryGetValue( typeof( TKey ), out var cacheRef ) )
            return new Dictionary<Type, DependencyResolver>( _defaultResolvers );

        var cache = ReinterpretCast.To<Cache<TKey>>( cacheRef );
        return cache.Resolvers.TryGetValue( key, out var resolvers )
            ? resolvers
            : new Dictionary<Type, DependencyResolver>( _defaultResolvers );
    }

    private sealed class Cache<TKey>
        where TKey : notnull
    {
        internal readonly Dictionary<TKey, Dictionary<Type, DependencyResolver>> Resolvers;

        internal Cache()
        {
            Resolvers = new Dictionary<TKey, Dictionary<Type, DependencyResolver>>();
        }
    }
}
