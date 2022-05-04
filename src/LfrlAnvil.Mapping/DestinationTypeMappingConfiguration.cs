using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public class DestinationTypeMappingConfiguration<TDestination> : IMappingConfiguration
    {
        private readonly Dictionary<Type, MappingStore> _stores;

        public DestinationTypeMappingConfiguration()
        {
            _stores = new Dictionary<Type, MappingStore>();
        }

        public Type DestinationType => typeof( TDestination );

        public DestinationTypeMappingConfiguration<TDestination> Configure<TSource>(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            _stores[typeof( TSource )] = MappingStore.Create( mapping );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<MappingKey, MappingStore>> GetMappingStores()
        {
            return _stores.Select( kv => KeyValuePair.Create( new MappingKey( kv.Key, DestinationType ), kv.Value ) );
        }
    }
}
