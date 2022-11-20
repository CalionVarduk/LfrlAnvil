using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal readonly struct DependencyLocatorBuilderStore
{
    private readonly Dictionary<Type, KeyedCache> _cachesByKeyType;

    private DependencyLocatorBuilderStore(DependencyLocatorBuilder global)
    {
        Global = global;
        _cachesByKeyType = new Dictionary<Type, KeyedCache>();
    }

    internal DependencyLocatorBuilder Global { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorBuilderStore Create()
    {
        return new DependencyLocatorBuilderStore( new DependencyLocatorBuilder() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyLocatorBuilder<TKey>? GetKeyed<TKey>(TKey key)
        where TKey : notnull
    {
        if ( ! _cachesByKeyType.TryGetValue( typeof( TKey ), out var cacheRef ) )
            return null;

        var cache = ReinterpretCast.To<KeyedCache<TKey>>( cacheRef );
        return cache.Locators.GetValueOrDefault( key );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyLocatorBuilder<TKey> GetOrAddKeyed<TKey>(TKey key)
        where TKey : notnull
    {
        if ( ! _cachesByKeyType.TryGetValue( typeof( TKey ), out var cacheRef ) )
        {
            cacheRef = new KeyedCache<TKey>();
            _cachesByKeyType.Add( typeof( TKey ), cacheRef );
        }

        var cache = ReinterpretCast.To<KeyedCache<TKey>>( cacheRef );
        if ( ! cache.Locators.TryGetValue( key, out var builder ) )
        {
            builder = new DependencyLocatorBuilder<TKey>( key );
            cache.Locators.Add( key, builder );
        }

        return builder;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IEnumerable<IKeyedDependencyLocatorBuilder> GetAllKeyed()
    {
        return _cachesByKeyType.Values.SelectMany( f => f.GetAll() );
    }

    private abstract class KeyedCache
    {
        [Pure]
        internal abstract IEnumerable<IKeyedDependencyLocatorBuilder> GetAll();
    }

    private sealed class KeyedCache<TKey> : KeyedCache
        where TKey : notnull
    {
        internal readonly Dictionary<TKey, DependencyLocatorBuilder<TKey>> Locators;

        internal KeyedCache()
        {
            Locators = new Dictionary<TKey, DependencyLocatorBuilder<TKey>>();
        }

        [Pure]
        internal override IEnumerable<IKeyedDependencyLocatorBuilder> GetAll()
        {
            return Locators.Values.Where( b => b.Dependencies.Count > 0 );
        }
    }
}
