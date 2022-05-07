using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public class SourceTypeMappingConfiguration<TSource> : ITypeMappingConfiguration
    {
        private readonly Dictionary<Type, TypeMappingStore> _stores;

        public SourceTypeMappingConfiguration()
        {
            _stores = new Dictionary<Type, TypeMappingStore>();
        }

        public Type SourceType => typeof( TSource );

        public SourceTypeMappingConfiguration<TSource> Configure<TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            _stores[typeof( TDestination )] = TypeMappingStore.Create( mapping );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
        {
            return _stores.Select( kv => KeyValuePair.Create( new TypeMappingKey( SourceType, kv.Key ), kv.Value ) );
        }
    }
}
