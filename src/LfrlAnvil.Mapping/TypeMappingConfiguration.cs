using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <inheritdoc cref="ITypeMappingConfiguration" />
public partial class TypeMappingConfiguration : ITypeMappingConfiguration
{
    private readonly Dictionary<TypeMappingKey, TypeMappingStore> _stores;

    /// <summary>
    /// Creates a new <see cref="TypeMappingConfiguration"/> instance without any mapping definitions.
    /// </summary>
    public TypeMappingConfiguration()
    {
        _stores = new Dictionary<TypeMappingKey, TypeMappingStore>();
    }

    /// <summary>
    /// Sets a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><b>this</b>.</returns>
    public TypeMappingConfiguration Configure<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        _stores[key] = TypeMappingStore.Create( mapping );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        return _stores;
    }
}
