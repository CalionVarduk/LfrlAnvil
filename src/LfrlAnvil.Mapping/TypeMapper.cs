using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <inheritdoc cref="ITypeMapper" />
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

    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> of <typeparamref name="TSource"/> type
    /// to the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> mapped to the <typeparamref name="TDestination"/> type
    /// if mapping was successful.
    /// </param>
    /// <typeparam name="TSource">Source object type.</typeparam>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
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

    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> to the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> mapped to the <typeparamref name="TDestination"/> type
    /// if mapping was successful.
    /// </param>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
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

    /// <summary>
    /// Attempts to map the provided <paramref name="source"/> collection with elements of <typeparamref name="TSource"/> type
    /// to a collection with elements of the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="result">
    /// <b>out</b> parameter that returns <paramref name="source"/> collection mapped to collection with elements
    /// of the <typeparamref name="TDestination"/> type if mapping was successful.
    /// </param>
    /// <typeparam name="TSource">Source collection's element type.</typeparam>
    /// <typeparam name="TDestination">Desired destination collection's element type.</typeparam>
    /// <returns><b>true</b> when mapping was successful, otherwise <b>false</b>.</returns>
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
