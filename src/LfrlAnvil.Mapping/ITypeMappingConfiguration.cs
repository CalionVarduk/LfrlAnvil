using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

public interface ITypeMappingConfiguration
{
    [Pure]
    IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores();
}