using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping
{
    public interface ITypeMapper
    {
        [Pure]
        TDestination Map<TSource, TDestination>(TSource source);

        [Pure]
        TDestination Map<TDestination>(object source);

        [Pure]
        MappingContext<TSource> Map<TSource>(TSource source);

        [Pure]
        object Map(Type destinationType, object source);

        [Pure]
        IEnumerable<TDestination> MapMany<TSource, TDestination>(IEnumerable<TSource> source);

        [Pure]
        IEnumerable<TDestination> MapMany<TSource, TDestination>(params TSource[] source);

        [Pure]
        MappingManyContext<TSource> MapMany<TSource>(IEnumerable<TSource> source);

        [Pure]
        MappingManyContext<TSource> MapMany<TSource>(params TSource[] source);

        bool TryMap<TSource, TDestination>(TSource source, [MaybeNullWhen( false )] out TDestination result);
        bool TryMap<TDestination>(object source, [MaybeNullWhen( false )] out TDestination result);
        bool TryMap(Type destinationType, object source, [MaybeNullWhen( false )] out object result);
        bool TryMapMany<TSource, TDestination>(IEnumerable<TSource> source, [MaybeNullWhen( false )] out IEnumerable<TDestination> result);

        [Pure]
        bool IsConfigured<TSource, TDestination>();

        [Pure]
        bool IsConfigured(Type sourceType, Type destinationType);

        [Pure]
        bool IsConfiguredAsSourceType<T>();

        [Pure]
        bool IsConfiguredAsSourceType(Type type);

        [Pure]
        bool IsConfiguredAsDestinationType<T>();

        [Pure]
        bool IsConfiguredAsDestinationType(Type type);

        [Pure]
        IEnumerable<MappingKey> GetConfiguredMappings();

        [Pure]
        IEnumerable<Type> GetConfiguredSourceTypes<TDestination>();

        [Pure]
        IEnumerable<Type> GetConfiguredSourceTypes(Type destinationType);

        [Pure]
        IEnumerable<Type> GetConfiguredDestinationTypes<TSource>();

        [Pure]
        IEnumerable<Type> GetConfiguredDestinationTypes(Type sourceType);

        [Pure]
        IEnumerable<Type> GetConfiguredSourceTypes();

        [Pure]
        IEnumerable<Type> GetConfiguredDestinationTypes();
    }
}
