using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a configuration of possibly multiple type mapping definitions.
/// </summary>
public interface ITypeMappingConfiguration
{
    /// <summary>
    /// Returns all type mapping definitions created by this configuration.
    /// </summary>
    /// <returns>All type mapping definitions created by this configuration.</returns>
    [Pure]
    IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores();
}
