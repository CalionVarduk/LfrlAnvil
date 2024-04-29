using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a builder of <see cref="ITypeMapper"/> instances.
/// </summary>
public interface ITypeMapperBuilder
{
    /// <summary>
    /// Adds an <see cref="ITypeMappingConfiguration"/> instance to this builder.
    /// </summary>
    /// <param name="configuration"><see cref="ITypeMappingConfiguration"/> instance to add to this builder.</param>
    /// <returns><b>this</b>.</returns>
    ITypeMapperBuilder Configure(ITypeMappingConfiguration configuration);

    /// <summary>
    /// Adds a collection of <see cref="ITypeMappingConfiguration"/> instances to this builder.
    /// </summary>
    /// <param name="configurations">A collection <see cref="ITypeMappingConfiguration"/> instances to add to this builder.</param>
    /// <returns><b>this</b>.</returns>
    ITypeMapperBuilder Configure(IEnumerable<ITypeMappingConfiguration> configurations);

    /// <summary>
    /// Returns all currently registered <see cref="ITypeMappingConfiguration"/> instances in this builder.
    /// </summary>
    /// <returns>All currently registered <see cref="ITypeMappingConfiguration"/> instances.</returns>
    [Pure]
    IEnumerable<ITypeMappingConfiguration> GetConfigurations();

    /// <summary>
    /// Creates a new <see cref="ITypeMapper"/> instance.
    /// </summary>
    /// <returns>New <see cref="ITypeMapper"/> instance.</returns>
    [Pure]
    ITypeMapper Build();
}
