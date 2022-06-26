using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

public class DestinationTypeMappingConfiguration<TDestination> : ITypeMappingConfiguration
{
    private readonly Dictionary<Type, TypeMappingStore> _stores;

    public DestinationTypeMappingConfiguration()
    {
        _stores = new Dictionary<Type, TypeMappingStore>();
    }

    public Type DestinationType => typeof( TDestination );

    public DestinationTypeMappingConfiguration<TDestination> Configure<TSource>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _stores[typeof( TSource )] = TypeMappingStore.Create( mapping );
        return this;
    }

    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _stores.Select( kv => KeyValuePair.Create( new TypeMappingKey( kv.Key, DestinationType ), kv.Value ) );
    }
}
