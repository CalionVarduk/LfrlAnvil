using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a configuration of possibly multiple type mapping definitions to a single <typeparamref name="TDestination"/> type.
/// </summary>
/// <typeparam name="TDestination">Destination type.</typeparam>
public class DestinationTypeMappingConfiguration<TDestination> : ITypeMappingConfiguration
{
    private readonly Dictionary<Type, TypeMappingStore> _stores;

    /// <summary>
    /// Creates a new <see cref="DestinationTypeMappingConfiguration{TDestination}"/> instance without any mapping definitions.
    /// </summary>
    public DestinationTypeMappingConfiguration()
    {
        _stores = new Dictionary<Type, TypeMappingStore>();
    }

    /// <summary>
    /// Destination type.
    /// </summary>
    public Type DestinationType => typeof( TDestination );

    /// <summary>
    /// Sets a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <returns><b>this</b>.</returns>
    public DestinationTypeMappingConfiguration<TDestination> Configure<TSource>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _stores[typeof( TSource )] = TypeMappingStore.Create( mapping );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _stores.Select( kv => KeyValuePair.Create( new TypeMappingKey( kv.Key, DestinationType ), kv.Value ) );
    }
}
