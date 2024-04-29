using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Represents a configuration of a single <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
/// </summary>
/// <typeparam name="TSource">Source type.</typeparam>
/// <typeparam name="TDestination">Destination type.</typeparam>
public class SingleTypeMappingConfiguration<TSource, TDestination> : ITypeMappingConfiguration
{
    private TypeMappingStore? _store;

    /// <summary>
    /// Creates a new <see cref="SingleTypeMappingConfiguration{TSource,TDestination}"/> instance without any mapping definition.
    /// </summary>
    public SingleTypeMappingConfiguration()
    {
        _store = null;
    }

    /// <summary>
    /// Creates a new <see cref="SingleTypeMappingConfiguration{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    public SingleTypeMappingConfiguration(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _store = TypeMappingStore.Create( mapping );
    }

    /// <summary>
    /// Source type.
    /// </summary>
    public Type SourceType => typeof( TSource );

    /// <summary>
    /// Destination type.
    /// </summary>
    public Type DestinationType => typeof( TDestination );

    /// <summary>
    /// Sets the current mapping definition.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <returns><b>this</b>.</returns>
    public SingleTypeMappingConfiguration<TSource, TDestination> Configure(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        _store = TypeMappingStore.Create( mapping );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<KeyValuePair<TypeMappingKey, TypeMappingStore>> GetMappingStores()
    {
        if ( _store is not null )
            yield return KeyValuePair.Create( new TypeMappingKey( SourceType, DestinationType ), _store.Value );
    }
}
