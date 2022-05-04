using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public class SourceTypeMappingConfiguration<TSource> : IMappingConfiguration
    {
        private readonly Dictionary<Type, MappingStore> _stores;

        public SourceTypeMappingConfiguration()
        {
            _stores = new Dictionary<Type, MappingStore>();
        }

        public Type SourceType => typeof( TSource );

        public SourceTypeMappingConfiguration<TSource> Configure<TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            _stores[typeof( TDestination )] = MappingStore.Create( mapping );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<MappingKey, MappingStore>> GetMappingStores()
        {
            return _stores.Select( kv => KeyValuePair.Create( new MappingKey( SourceType, kv.Key ), kv.Value ) );
        }
    }
}
