using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Mapping.Exceptions;
using LfrlAnvil.Mapping.Internal;

namespace LfrlAnvil.Mapping;

public sealed class TypeMapper : ITypeMapper
{
    private readonly Dictionary<TypeMappingKey, TypeMappingStore> _stores;

    internal TypeMapper(IEnumerable<ITypeMappingConfiguration> configurations)
    {
        _stores = new Dictionary<TypeMappingKey, TypeMappingStore>();
        var stores = configurations.SelectMany( c => c.GetMappingStores() );
        foreach ( var (key, value) in stores )
            _stores[key] = value;
    }

    [Pure]
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if ( ! TryMap<TSource, TDestination>( source, out var result ) )
            throw UndefinedTypeMappingException<TSource, TDestination>();

        return result;
    }

    [Pure]
    public TDestination Map<TDestination>(object source)
    {
        if ( ! TryMap<TDestination>( source, out var result ) )
            throw UndefinedTypeMappingException<TDestination>( source.GetType() );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeMappingContext<TSource> Map<TSource>(TSource source)
    {
        return new TypeMappingContext<TSource>( this, source );
    }

    [Pure]
    public object Map(Type destinationType, object source)
    {
        if ( ! TryMap( destinationType, source, out var result ) )
            throw UndefinedTypeMappingException( source.GetType(), destinationType );

        return result;
    }

    [Pure]
    public IEnumerable<TDestination> MapMany<TSource, TDestination>(IEnumerable<TSource> source)
    {
        if ( ! TryMapMany<TSource, TDestination>( source, out var result ) )
            throw UndefinedTypeMappingException<TSource, TDestination>();

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerable<TDestination> MapMany<TSource, TDestination>(params TSource[] source)
    {
        return MapMany<TSource, TDestination>( source.AsEnumerable() );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeMappingManyContext<TSource> MapMany<TSource>(IEnumerable<TSource> source)
    {
        return new TypeMappingManyContext<TSource>( this, source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TypeMappingManyContext<TSource> MapMany<TSource>(params TSource[] source)
    {
        return MapMany( source.AsEnumerable() );
    }

    public bool TryMap<TSource, TDestination>(TSource source, [MaybeNullWhen( false )] out TDestination result)
    {
        var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate<TSource, TDestination>();
        result = mapping( source, this );
        return true;
    }

    public bool TryMap<TDestination>(object source, [MaybeNullWhen( false )] out TDestination result)
    {
        var key = new TypeMappingKey( source.GetType(), typeof( TDestination ) );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate<TDestination>();
        result = mapping( source, this );
        return true;
    }

    public bool TryMap(Type destinationType, object source, [MaybeNullWhen( false )] out object result)
    {
        var key = new TypeMappingKey( source.GetType(), destinationType );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate();
        result = mapping( source, this );
        return true;
    }

    public bool TryMapMany<TSource, TDestination>(
        IEnumerable<TSource> source,
        [MaybeNullWhen( false )] out IEnumerable<TDestination> result)
    {
        var key = new TypeMappingKey( typeof( TSource ), typeof( TDestination ) );
        if ( ! _stores.TryGetValue( key, out var store ) )
        {
            result = default;
            return false;
        }

        var mapping = store.GetDelegate<TSource, TDestination>();
        result = source.Select( s => mapping( s, this ) );
        return true;
    }

    [Pure]
    public bool IsConfigured<TSource, TDestination>()
    {
        return IsConfigured( typeof( TSource ), typeof( TDestination ) );
    }

    [Pure]
    public bool IsConfigured(Type sourceType, Type destinationType)
    {
        return _stores.ContainsKey( new TypeMappingKey( sourceType, destinationType ) );
    }

    [Pure]
    public bool IsConfiguredAsSourceType<T>()
    {
        return IsConfiguredAsSourceType( typeof( T ) );
    }

    [Pure]
    public bool IsConfiguredAsSourceType(Type type)
    {
        return _stores.Any( kv => kv.Key.SourceType == type );
    }

    [Pure]
    public bool IsConfiguredAsDestinationType<T>()
    {
        return IsConfiguredAsDestinationType( typeof( T ) );
    }

    [Pure]
    public bool IsConfiguredAsDestinationType(Type type)
    {
        return _stores.Any( kv => kv.Key.DestinationType == type );
    }

    [Pure]
    public IEnumerable<TypeMappingKey> GetConfiguredMappings()
    {
        return _stores.Keys;
    }

    [Pure]
    public IEnumerable<Type> GetConfiguredSourceTypes<TDestination>()
    {
        return GetConfiguredSourceTypes( typeof( TDestination ) );
    }

    [Pure]
    public IEnumerable<Type> GetConfiguredSourceTypes(Type destinationType)
    {
        return _stores
            .Where( kv => kv.Key.DestinationType == destinationType )
            .Select( kv => kv.Key.SourceType! );
    }

    [Pure]
    public IEnumerable<Type> GetConfiguredDestinationTypes<TSource>()
    {
        return GetConfiguredDestinationTypes( typeof( TSource ) );
    }

    [Pure]
    public IEnumerable<Type> GetConfiguredDestinationTypes(Type sourceType)
    {
        return _stores
            .Where( kv => kv.Key.SourceType == sourceType )
            .Select( kv => kv.Key.DestinationType! );
    }

    [Pure]
    public IEnumerable<Type> GetConfiguredSourceTypes()
    {
        return _stores.Select( kv => kv.Key.SourceType! ).Distinct();
    }

    [Pure]
    public IEnumerable<Type> GetConfiguredDestinationTypes()
    {
        return _stores.Select( kv => kv.Key.DestinationType! ).Distinct();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static UndefinedTypeMappingException UndefinedTypeMappingException<TSource, TDestination>()
    {
        return UndefinedTypeMappingException( typeof( TSource ), typeof( TDestination ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static UndefinedTypeMappingException UndefinedTypeMappingException<TDestination>(Type sourceType)
    {
        return UndefinedTypeMappingException( sourceType, typeof( TDestination ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static UndefinedTypeMappingException UndefinedTypeMappingException(Type sourceType, Type destinationType)
    {
        return new UndefinedTypeMappingException( sourceType, destinationType );
    }
}