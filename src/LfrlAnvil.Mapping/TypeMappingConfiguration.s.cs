using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Mapping;

public partial class TypeMappingConfiguration
{
    /// <summary>
    /// Creates a new <see cref="SingleTypeMappingConfiguration{TSource,TDestination}"/> instance.
    /// </summary>
    /// <param name="mapping"><typeparamref name="TSource"/> => <typeparamref name="TDestination"/> mapping definition.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns>New <see cref="SingleTypeMappingConfiguration{TSource,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SingleTypeMappingConfiguration<TSource, TDestination> Create<TSource, TDestination>(
        Func<TSource, ITypeMapper, TDestination> mapping)
    {
        return new SingleTypeMappingConfiguration<TSource, TDestination>( mapping );
    }
}
