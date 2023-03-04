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

    private readonly DependencyResolverFactory[] _defaultResolverFactories;
    private readonly Dictionary<(IDependencyKey, DependencyLifetime), DependencyResolverFactory> _sharedResolvers;

    private DependencyLocatorBuilderExtractionParams(UlongSequenceGenerator idGenerator)
    {
        _sharedResolvers = new Dictionary<(IDependencyKey, DependencyLifetime), DependencyResolverFactory>();

        _defaultResolverFactories = new[]
        {
            DependencyResolverFactory.CreateFinished(
                ImplementorKey.CreateShared( new DependencyKey( typeof( IDependencyContainer ) ) ),
                DependencyLifetime.Singleton,
                new DependencyContainerResolver( idGenerator.Generate() ) ),
            DependencyResolverFactory.CreateFinished(
                ImplementorKey.CreateShared( new DependencyKey( typeof( IDependencyScope ) ) ),
                DependencyLifetime.Singleton,
                new DependencyScopeResolver( idGenerator.Generate() ) )
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
        foreach ( var factory in _defaultResolverFactories )
            ResolverFactories[locatorBuilder.CreateImplementorKey( factory.ImplementorKey.Value.Type )] = factory;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IReadOnlyDictionary<Type, DependencyResolver> GetDefaultResolvers()
    {
        var result = new Dictionary<Type, DependencyResolver>();
        foreach ( var factory in _defaultResolverFactories )
            result.Add( factory.ImplementorKey.Value.Type, factory.GetResolver() );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorBuilderExtractionParams Create(UlongSequenceGenerator idGenerator)
    {
        return new DependencyLocatorBuilderExtractionParams( idGenerator );
    }
}
