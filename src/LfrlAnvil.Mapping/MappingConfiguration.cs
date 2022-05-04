using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public class MappingConfiguration : IMappingConfiguration
    {
        private readonly Dictionary<MappingKey, MappingStore> _stores;

        public MappingConfiguration()
        {
            _stores = new Dictionary<MappingKey, MappingStore>();
        }

        public MappingConfiguration Configure<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            var key = new MappingKey( typeof( TSource ), typeof( TDestination ) );
            _stores[key] = MappingStore.Create( mapping );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<MappingKey, MappingStore>> GetMappingStores()
        {
            return _stores;
        }
    }
}
