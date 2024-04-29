using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Mapping;

/// <inheritdoc />
public class TypeMapperBuilder : ITypeMapperBuilder
{
    private readonly List<ITypeMappingConfiguration> _configurations;

    /// <summary>
    /// Creates a new empty <see cref="TypeMapperBuilder"/> instance.
    /// </summary>
    public TypeMapperBuilder()
    {
        _configurations = new List<ITypeMappingConfiguration>();
    }

    /// <inheritdoc />
    public ITypeMapperBuilder Configure(ITypeMappingConfiguration configuration)
    {
        _configurations.Add( configuration );
        return this;
    }

    /// <inheritdoc />
    public ITypeMapperBuilder Configure(IEnumerable<ITypeMappingConfiguration> configurations)
    {
        _configurations.AddRange( configurations );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<ITypeMappingConfiguration> GetConfigurations()
    {
        return _configurations;
    }

    /// <inheritdoc />
    [Pure]
    public ITypeMapper Build()
    {
        return new TypeMapper( _configurations );
    }
}
