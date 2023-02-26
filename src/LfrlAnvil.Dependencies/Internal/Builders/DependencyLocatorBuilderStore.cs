using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
        ref var cacheRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _cachesByKeyType, typeof( TKey ), out var exists )!;
        if ( ! exists )
            cacheRef = new KeyedCache<TKey>();

        var cache = ReinterpretCast.To<KeyedCache<TKey>>( cacheRef );
        ref var builder = ref CollectionsMarshal.GetValueRefOrAddDefault( cache.Locators, key, out exists )!;
        if ( ! exists )
            builder = new DependencyLocatorBuilder<TKey>( key );

        return builder;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IEnumerable<DependencyLocatorBuilder> GetAll()
    {
        return _cachesByKeyType.Values.SelectMany( static f => f.GetAll() ).Prepend( Global );
    }

    private abstract class KeyedCache
    {
        [Pure]
        internal abstract IEnumerable<DependencyLocatorBuilder> GetAll();
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
        internal override IEnumerable<DependencyLocatorBuilder> GetAll()
        {
            return Locators.Values.Where( static b => b.Dependencies.Count > 0 );
        }
    }
}
