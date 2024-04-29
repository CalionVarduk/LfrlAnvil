using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Mapping.Exceptions;

namespace LfrlAnvil.Mapping;

/// <summary>
/// Contains <see cref="ITypeMapper"/> extension methods.
/// </summary>
public static class TypeMapperExtensions
{
    /// <summary>
    /// Maps the provided <paramref name="source"/> of <typeparamref name="TSource"/> type
    /// to the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source object.</param>
    /// <typeparam name="TSource">Source object type.</typeparam>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><paramref name="source"/> mapped to the <typeparamref name="TDestination"/> type.</returns>
    /// <exception cref="UndefinedTypeMappingException">
    /// When mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/> is not defined.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDestination Map<TSource, TDestination>(this ITypeMapper mapper, TSource source)
    {
        if ( ! mapper.TryMap<TSource, TDestination>( source, out var result ) )
            ThrowUndefinedTypeMappingException( typeof( TSource ), typeof( TDestination ) );

        return result;
    }

    /// <summary>
    /// Maps the provided <paramref name="source"/> to the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source object.</param>
    /// <typeparam name="TDestination">Desired destination type.</typeparam>
    /// <returns><paramref name="source"/> mapped to the <typeparamref name="TDestination"/> type.</returns>
    /// <exception cref="UndefinedTypeMappingException">
    /// When mapping from <paramref name="source"/> object's type to <typeparamref name="TDestination"/> is not defined.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TDestination Map<TDestination>(this ITypeMapper mapper, object source)
    {
        if ( ! mapper.TryMap<TDestination>( source, out var result ) )
            ThrowUndefinedTypeMappingException( source.GetType(), typeof( TDestination ) );

        return result;
    }

    /// <summary>
    /// Maps the provided <paramref name="source"/> to the desired <paramref name="destinationType"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="destinationType">Desired destination type.</param>
    /// <param name="source">Source object.</param>
    /// <returns><paramref name="source"/> mapped to the <paramref name="destinationType"/> type.</returns>
    /// <exception cref="UndefinedTypeMappingException">
    /// When mapping from <paramref name="source"/> object's type to <paramref name="destinationType"/> is not defined.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static object Map(this ITypeMapper mapper, Type destinationType, object source)
    {
        if ( ! mapper.TryMap( destinationType, source, out var result ) )
            ThrowUndefinedTypeMappingException( source.GetType(), destinationType );

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="TypeMappingContext{TSource}"/> instance
    /// for the provided <paramref name="source"/> of <typeparamref name="TSource"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source object.</param>
    /// <typeparam name="TSource">Source object type.</typeparam>
    /// <returns>New <see cref="TypeMappingContext{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TypeMappingContext<TSource> Map<TSource>(this ITypeMapper mapper, TSource source)
    {
        return new TypeMappingContext<TSource>( mapper, source );
    }

    /// <summary>
    /// Maps the provided <paramref name="source"/> collection with elements of <typeparamref name="TSource"/> type
    /// to a collection with elements of the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TSource">Source collection's element type.</typeparam>
    /// <typeparam name="TDestination">Desired destination collection's element type.</typeparam>
    /// <returns>
    /// <paramref name="source"/> collection mapped to collection with elements of the <typeparamref name="TDestination"/> type.
    /// </returns>
    /// <exception cref="UndefinedTypeMappingException">
    /// When mapping from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/> is not defined.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TDestination> MapMany<TSource, TDestination>(this ITypeMapper mapper, IEnumerable<TSource> source)
    {
        if ( ! mapper.TryMapMany<TSource, TDestination>( source, out var result ) )
            ThrowUndefinedTypeMappingException( typeof( TSource ), typeof( TDestination ) );

        return result;
    }

    /// <summary>
    /// Maps the provided <paramref name="source"/> collection with elements of <typeparamref name="TSource"/> type
    /// to a collection with elements of the desired <typeparamref name="TDestination"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TSource">Source collection's element type.</typeparam>
    /// <typeparam name="TDestination">Desired destination collection's element type.</typeparam>
    /// <returns>
    /// <paramref name="source"/> collection mapped to collection with elements of the <typeparamref name="TDestination"/> type.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TDestination> MapMany<TSource, TDestination>(this ITypeMapper mapper, params TSource[] source)
    {
        return mapper.MapMany<TSource, TDestination>( source.AsEnumerable() );
    }

    /// <summary>
    /// Creates a new <see cref="TypeMappingManyContext{TSource}"/> instance
    /// for the provided <paramref name="source"/> collection with elements of <typeparamref name="TSource"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TSource">Source collection's element type.</typeparam>
    /// <returns>New <see cref="TypeMappingManyContext{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TypeMappingManyContext<TSource> MapMany<TSource>(this ITypeMapper mapper, IEnumerable<TSource> source)
    {
        return new TypeMappingManyContext<TSource>( mapper, source );
    }

    /// <summary>
    /// Creates a new <see cref="TypeMappingManyContext{TSource}"/> instance
    /// for the provided <paramref name="source"/> collection with elements of <typeparamref name="TSource"/> type.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TSource">Source collection's element type.</typeparam>
    /// <returns>New <see cref="TypeMappingManyContext{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TypeMappingManyContext<TSource> MapMany<TSource>(this ITypeMapper mapper, params TSource[] source)
    {
        return mapper.MapMany( source.AsEnumerable() );
    }

    /// <summary>
    /// Checks whether or not the mapping definition from <typeparamref name="TSource"/> to <typeparamref name="TDestination"/> exists.
    /// </summary>
    /// <param name="mapper">Type mapper.</param>
    /// <typeparam name="TSource">Source type.</typeparam>
    /// <typeparam name="TDestination">Destination type.</typeparam>
    /// <returns><b>true</b> when mapping definition exists, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool IsConfigured<TSource, TDestination>(this ITypeMapper mapper)
    {
        return mapper.IsConfigured( typeof( TSource ), typeof( TDestination ) );
    }

    /// <summary>
    /// Adds a collection of <see cref="ITypeMappingConfiguration"/> instances to this builder.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="configurations">A collection <see cref="ITypeMappingConfiguration"/> instances to add to this builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ITypeMapperBuilder Configure(this ITypeMapperBuilder builder, params ITypeMappingConfiguration[] configurations)
    {
        return builder.Configure( configurations.AsEnumerable() );
    }

    [DoesNotReturn]
    [StackTraceHidden]
    private static void ThrowUndefinedTypeMappingException(Type sourceType, Type destinationType)
    {
        throw new UndefinedTypeMappingException( sourceType, destinationType );
    }
}
