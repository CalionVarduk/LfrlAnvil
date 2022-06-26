using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Mapping;

public interface ITypeMapperBuilder
{
    ITypeMapperBuilder Configure(ITypeMappingConfiguration configuration);
    ITypeMapperBuilder Configure(params ITypeMappingConfiguration[] configurations);
    ITypeMapperBuilder Configure(IEnumerable<ITypeMappingConfiguration> configurations);

    [Pure]
    IEnumerable<ITypeMappingConfiguration> GetConfigurations();

    [Pure]
    ITypeMapper Build();
}