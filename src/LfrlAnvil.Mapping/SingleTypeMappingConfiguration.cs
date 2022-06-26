using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

public class SingleTypeMappingConfiguration<TSource, TDestination> : ITypeMappingConfiguration
{
    private TypeMappingStore? _store;

    public SingleTypeMappingConfiguration()
    {
        _store = null;
    }

    public SingleTypeMappingConfiguration(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _store = TypeMappingStore.Create( mapping );
    }

    public Type SourceType => typeof( TSource );
    public Type DestinationType => typeof( TDestination );

    public SingleTypeMappingConfiguration<TSource, TDestination> Configure(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _store = TypeMappingStore.Create( mapping );
        return this;
    }

    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        if ( _store is not null )
            yield return KeyValuePair.Create( new TypeMappingKey( SourceType, DestinationType ), _store.Value );
    }
}