using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Mapping;

public class TypeMapperBuilder : ITypeMapperBuilder
{
    private readonly List<ITypeMappingConfiguration> _configurations;

    public TypeMapperBuilder()
    {
        _configurations = new List<ITypeMappingConfiguration>();
    }

    public ITypeMapperBuilder Configure(ITypeMappingConfiguration configuration)
    {
        _configurations.Add( configuration );
        return this;
    }

    public ITypeMapperBuilder Configure(params ITypeMappingConfiguration[] configurations)
    {
        return Configure( configurations.AsEnumerable() );
    }

    public ITypeMapperBuilder Configure(IEnumerable<ITypeMappingConfiguration> configurations)
    {
        _configurations.AddRange( configurations );
        return this;
    }

    [Pure]
    public IEnumerable<ITypeMappingConfiguration> GetConfigurations()
    {
        return _configurations;
    }

    [Pure]
    public ITypeMapper Build()
    {
        return new TypeMapper( _configurations );
    }
}