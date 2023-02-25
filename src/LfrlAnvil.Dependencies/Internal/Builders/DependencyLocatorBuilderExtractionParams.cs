using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal readonly struct DependencyLocatorBuilderExtractionParams
{
    internal readonly Dictionary<IDependencyKey, DependencyResolverFactory> ResolverFactories;

    private readonly KeyValuePair<Type, DependencyResolverFactory>[] _defaultResolverFactories;
    private readonly Dictionary<(IDependencyKey, DependencyLifetime), DependencyResolverFactory> _sharedResolvers;

    private DependencyLocatorBuilderExtractionParams(UlongSequenceGenerator idGenerator)
    {
        _sharedResolvers = new Dictionary<(IDependencyKey, DependencyLifetime), DependencyResolverFactory>();

        _defaultResolverFactories = new[]
        {
            KeyValuePair.Create<Type, DependencyResolverFactory>(
                typeof( IDependencyContainer ),
                new InternalDependencyResolverFactory( new DependencyContainerResolver( idGenerator.Generate() ) ) ),
            KeyValuePair.Create<Type, DependencyResolverFactory>(
                typeof( IDependencyScope ),
                new InternalDependencyResolverFactory( new DependencyScopeResolver( idGenerator.Generate() ) ) )
        };

        ResolverFactories = new Dictionary<IDependencyKey, DependencyResolverFactory>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolverFactory? GetSharedResolverFactory(IDependencyKey key, DependencyLifetime lifetime)
    {
        return _sharedResolvers.GetValueOrDefault( (key, lifetime) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddSharedResolverFactory(IDependencyKey key, DependencyLifetime lifetime, DependencyResolverFactory factory)
    {
        _sharedResolvers.Add( (key, lifetime), factory );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddDefaultResolverFactories(DependencyLocatorBuilder locatorBuilder)
    {
        foreach ( var (type, factory) in _defaultResolverFactories )
            ResolverFactories.Add( locatorBuilder.CreateImplementorKey( type ), factory );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Dictionary<Type, DependencyResolver> GetDefaultResolvers()
    {
        var result = new Dictionary<Type, DependencyResolver>();
        foreach ( var (type, factory) in _defaultResolverFactories )
            result.Add( type, factory.GetResolver() );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorBuilderExtractionParams Create(UlongSequenceGenerator idGenerator)
    {
        return new DependencyLocatorBuilderExtractionParams( idGenerator );
    }
}
