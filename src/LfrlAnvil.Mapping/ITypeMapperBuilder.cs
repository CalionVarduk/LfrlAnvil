using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Mapping
{
    public interface ITypeMapperBuilder
    {
        ITypeMapperBuilder Configure(IMappingConfiguration configuration);
        ITypeMapperBuilder Configure(params IMappingConfiguration[] configurations);
        ITypeMapperBuilder Configure(IEnumerable<IMappingConfiguration> configurations);

        [Pure]
        IEnumerable<IMappingConfiguration> GetConfigurations();

        [Pure]
        ITypeMapper Build();
    }
}
