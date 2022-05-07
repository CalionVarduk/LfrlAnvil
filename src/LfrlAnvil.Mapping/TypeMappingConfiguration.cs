using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public partial class TypeMappingConfiguration : ITypeMappingConfiguration
    {
        private readonly Dictionary<TypeMappingKey, TypeMappingStore> _stores;

        public TypeMappingConfiguration()
        {
            _stores = new Dictionary<TypeMappingKey, TypeMappingStore>();
        }

        public TypeMappingConfiguration Configure<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
        {
            var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
            _stores[key] = TypeMappingStore.Create( mapping );
            return this;
        }

        [Pure]
        public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
        {
            return _stores;
        }
    }
}
