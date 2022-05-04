using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public class TypeMappingConfiguration<TSource, TDestination> : IMappingConfiguration
    {
        private MappingStore? _store;

        public TypeMappingConfiguration()
        {
            _store = null;
        }

        public TypeMappingConfiguration(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            _store = MappingStore.Create( mapping );
        }

        public Type SourceType => typeof( TSource );
        public Type DestinationType => typeof( TDestination );

        public TypeMappingConfiguration<TSource, TDestination> Configure(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            _store = MappingStore.Create( mapping );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<MappingKey, MappingStore>> GetMappingStores()
        {
            if ( _store is not null )
                yield return KeyValuePair.Create( new MappingKey( SourceType, DestinationType ), _store.Value );
        }
    }
}
