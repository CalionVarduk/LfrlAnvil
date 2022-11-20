using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Resolvers;

namespace LfrlAnvil.Dependencies.Internal;

internal readonly struct KeyedDependencyResolversStore
{
    private readonly Dictionary<Type, object> _cachesByKeyType;
    private readonly Dictionary<Type, DependencyResolver> _defaultResolvers;

    private KeyedDependencyResolversStore(Dictionary<Type, DependencyResolver> defaultResolvers)
    {
        _defaultResolvers = defaultResolvers;
        _cachesByKeyType = new Dictionary<Type, object>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static KeyedDependencyResolversStore Create(Dictionary<Type, DependencyResolver> defaultResolvers)
    {
        return new KeyedDependencyResolversStore( defaultResolvers );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddResolvers<TKey>(TKey key, Dictionary<Type, DependencyResolver> resolvers)
        where TKey : notnull
    {
        if ( ! _cachesByKeyType.TryGetValue( typeof( TKey ), out var cacheRef ) )
        {
            cacheRef = new Cache<TKey>();
            _cachesByKeyType.Add( typeof( TKey ), cacheRef );
        }

        var cache = ReinterpretCast.To<Cache<TKey>>( cacheRef );
        cache.Resolvers.Add( key, resolvers );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Dictionary<Type, DependencyResolver> GetResolvers<TKey>(TKey key)
        where TKey : notnull
    {
        if ( ! _cachesByKeyType.TryGetValue( typeof( TKey ), out var cacheRef ) )
            return _defaultResolvers;

        var cache = ReinterpretCast.To<Cache<TKey>>( cacheRef );
        return cache.Resolvers.TryGetValue( key, out var resolvers ) ? resolvers : _defaultResolvers;
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
