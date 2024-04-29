using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <inheritdoc />
public sealed class TypeMapper : ITypeMapper
{
    private readonly Dictionary<TypeMappingKey, TypeMappingStore> _stores;

    internal TypeMapper(IEnumerable<ITypeMappingConfiguration> configurations)
    {
        _stores = new Dictionary<TypeMappingKey, TypeMappingStore>();
        var stores = configurations.SelectMany( static c => c.GetMappingStores() );
        foreach ( var (key, value) in stores )
            _stores[key] = value;
    }

    /// <inheritdoc />
    public bool TryMap<TSource, TDestination>(TSource source, [MaybeNullWhen( false )] out TDestination result)
    {
        var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate<TSource, TDestination>();
        result = mapping( source, this );
        return true;
    }

    /// <inheritdoc />
    public bool TryMap<TDestination>(object source, [MaybeNullWhen( false )] out TDestination result)
    {
        var key = new TypeMappingKey( source.GetType(), typeof( TDestination ) );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate<TDestination>();
        result = mapping( source, this );
        return true;
    }

    /// <inheritdoc />
    public bool TryMap(Type destinationType, object source, [MaybeNullWhen( false )] out object result)
    {
        var key = new TypeMappingKey( source.GetType(), destinationType );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate();
        result = mapping( source, this );
        return true;
    }

    /// <inheritdoc />
    public bool TryMapMany<TSource, TDestination>(
        IEnumerable<TSource> source,
        [MaybeNullWhen( false )] out IEnumerable<TDestination> result)
    {
        var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate<TSource, TDestination>();
        result = source.Select( s => mapping( s, this ) );
        return true;
    }

    /// <inheritdoc />
    [Pure]
    public bool IsConfigured(Type sourceType, Type destinationType)
    {
        return _stores.ContainsKey( new TypeMappingKey( sourceType, destinationType ) );
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<TypeMappingKey> GetConfiguredMappings()
    {
        return _stores.Keys;
    }
}
