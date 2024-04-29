using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a configuration of possibly multiple type mapping definitions from a single <typeparamref name="TSource"/> type.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
public class SourceTypeMappingConfiguration<TSource> : ITypeMappingConfiguration
{
    private readonly Dictionary<Type, TypeMappingStore> _stores;

    /// <summary>
    /// Creates a new <see cref="SourceTypeMappingConfiguration{TSource}"/> instance without any mapping definitions.
    /// </summary>
    public SourceTypeMappingConfiguration()
    {
        _stores = new Dictionary<Type, TypeMappingStore>();
    }

    /// <summary>
    /// Source type.
    /// </summary>
    public Type SourceType => typeof( TSource );

    /// <summary>
    /// Sets a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><b>this</b>.</returns>
    public SourceTypeMappingConfiguration<TSource> Configure<TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _stores[typeof( TDestination )] = TypeMappingStore.Create( mapping );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _stores.Select( kv => KeyValuePair.Create( new TypeMappingKey( SourceType, kv.Key ), kv.Value ) );
    }
}
