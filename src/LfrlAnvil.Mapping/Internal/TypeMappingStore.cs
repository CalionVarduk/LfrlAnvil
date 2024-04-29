using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping.Internal;

/// <summary>
/// Represents a container for delegates used for type mapping.
/// </summary>
public readonly struct TypeMappingStore
{
    private TypeMappingStore(Delegate fastDelegate, Delegate slowDelegate)
    {
        FastDelegate = fastDelegate;
        SlowDelegate = slowDelegate;
    }

    /// <summary>
    /// Fast delegate. Used when both source and destination types are known.
    /// </summary>
    public Delegate FastDelegate { get; }

    /// <summary>
    /// Slow delegate. Used when either source or destination type is unknown.
    /// </summary>
    public Delegate SlowDelegate { get; }

    /// <summary>
    /// Returns the <see cref="FastDelegate"/>.
    /// </summary>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><see cref="FastDelegate"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// When <see cref="FastDelegate"/> is not a <typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Func<TSource, ITypeMapper, TDestination> GetDelegate<TSource, TDestination>()
    {
        return ( Func<TSource, ITypeMapper, TDestination> )FastDelegate;
    }

    /// <summary>
    /// Returns the <see cref="SlowDelegate"/>.
    /// </summary>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><see cref="SlowDelegate"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// When <see cref="SlowDelegate"/> is not an <see cref="Object"/> => <typeparamref name="TDestination"/> mapping definition.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Func<object, ITypeMapper, TDestination> GetDelegate<TDestination>()
    {
        return ( Func<object, ITypeMapper, TDestination> )SlowDelegate;
    }

    /// <summary>
    /// Returns the <see cref="SlowDelegate"/>.
    /// </summary>
    /// <returns><see cref="SlowDelegate"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// When <see cref="SlowDelegate"/> is not an <see cref="Object"/> => <see cref="Object"/> mapping definition.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Func<object, ITypeMapper, object> GetDelegate()
    {
        return ( Func<object, ITypeMapper, object> )SlowDelegate;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static TypeMappingStore Create<TSource, TDestination>(Func<TSource, ITypeMapper, TDestination> mapping)
    {
        Func<object, ITypeMapper, TDestination> slowMapping = (source, provider) => mapping( ( TSource )source, provider );
        return new TypeMappingStore( mapping, slowMapping );
    }
}
