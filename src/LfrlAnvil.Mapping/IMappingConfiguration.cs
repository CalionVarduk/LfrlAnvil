using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public interface IMappingConfiguration
    {
        [Pure]
        IEnumerable<KeyValuePair<MappingKey, MappingStore>> GetMappingStores();
    }
}
