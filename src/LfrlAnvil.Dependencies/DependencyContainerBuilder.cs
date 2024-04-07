using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

public class DependencyContainerBuilder : IDependencyContainerBuilder
{
    private readonly DependencyLocatorBuilderStore _locatorBuilderStore;

    public DependencyContainerBuilder()
    {
        _locatorBuilderStore = DependencyLocatorBuilderStore.Create();
        Configuration = new DependencyContainerConfigurationBuilder();
    }

    public IDependencyContainerConfigurationBuilder Configuration { get; }
    public DependencyLifetime DefaultLifetime => _locatorBuilderStore.Global.DefaultLifetime;
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy => _locatorBuilderStore.Global.DefaultDisposalStrategy;

    Type? IDependencyLocatorBuilder.KeyType => (( IDependencyLocatorBuilder )_locatorBuilderStore.Global).KeyType;
    object? IDependencyLocatorBuilder.Key => (( IDependencyLocatorBuilder )_locatorBuilderStore.Global).Key;
    bool IDependencyLocatorBuilder.IsKeyed => (( IDependencyLocatorBuilder )_locatorBuilderStore.Global).IsKeyed;

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        return _locatorBuilderStore.Global.AddSharedImplementor( type );
    }

    public IDependencyBuilder Add(Type type)
    {
        return _locatorBuilderStore.Global.Add( type );
    }

    public DependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        _locatorBuilderStore.Global.SetDefaultLifetime( lifetime );
        return this;
    }

    public DependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        _locatorBuilderStore.Global.SetDefaultDisposalStrategy( strategy );
        return this;
    }

    [Pure]
    public IDependencyImplementorBuilder? TryGetSharedImplementor(Type type)
    {
        return _locatorBuilderStore.Global.TryGetSharedImplementor( type );
    }

    [Pure]
    public IDependencyRangeBuilder GetDependencyRange(Type type)
    {
        return _locatorBuilderStore.Global.GetDependencyRange( type );
    }

    public IDependencyLocatorBuilder<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull
    {
        return _locatorBuilderStore.GetOrAddKeyed( key );
    }

    [Pure]
    public DependencyContainerBuildResult<DependencyContainer> TryBuild()
    {
        var idGenerator = new UlongSequenceGenerator();
        var extractionParams = DependencyLocatorBuilderExtractionParams.Create( idGenerator );
        var messages = Chain<DependencyContainerBuildMessages>.Empty;

        var locatorBuilders = _locatorBuilderStore.GetAll();
        foreach ( var locatorBuilder in locatorBuilders )
            messages = messages.Extend( locatorBuilder.ExtractResolverFactories( _locatorBuilderStore, extractionParams ) );

        var resolverFactories = extractionParams.ResolverFactories.Values;
        foreach ( var factory in resolverFactories )
            factory.PrepareCreationMethod( idGenerator, extractionParams.ResolverFactories, Configuration );

        foreach ( var factory in resolverFactories )
            factory.ValidateRequiredDependencies( extractionParams.ResolverFactories, Configuration );

        var pathBuffer = new List<DependencyGraphNode>();
        foreach ( var factory in resolverFactories )
            factory.ValidateCircularDependencies( pathBuffer );

        foreach ( var factory in resolverFactories )
        {
            var factoryMessages = factory.GetMessages();
            messages = messages.Extend( factoryMessages );
        }

        foreach ( var message in messages )
        {
            if ( message.Errors.Count > 0 )
                return new DependencyContainerBuildResult<DependencyContainer>( null, messages );
        }

        foreach ( var factory in resolverFactories )
            factory.Build( idGenerator );

        var defaultResolvers = extractionParams.GetDefaultResolvers();
        var globalDependencyResolvers = new Dictionary<Type, DependencyResolver>( defaultResolvers );
        var keyedDependencyResolvers = KeyedDependencyResolversStore.Create( defaultResolvers );

        foreach ( var (dependencyKey, factory) in extractionParams.ResolverFactories )
        {
            var resolvers = ReinterpretCast.To<IInternalDependencyKey>( dependencyKey )
                .GetTargetResolvers( globalDependencyResolvers, keyedDependencyResolvers );

            resolvers.TryAdd( dependencyKey.Type, factory.GetResolver() );
        }

        var result = new DependencyContainer( idGenerator, globalDependencyResolvers, keyedDependencyResolvers );
        return new DependencyContainerBuildResult<DependencyContainer>( result, messages );
    }

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> IDependencyContainerBuilder.TryBuild()
    {
        var result = TryBuild();
        return new DependencyContainerBuildResult<IDisposableDependencyContainer>( result.Container, result.Messages );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetDefaultLifetime(DependencyLifetime lifetime)
    {
        return SetDefaultLifetime( lifetime );
    }

    IDependencyLocatorBuilder IDependencyLocatorBuilder.SetDefaultLifetime(DependencyLifetime lifetime)
    {
        return ReinterpretCast.To<IDependencyContainerBuilder>( this ).SetDefaultLifetime( lifetime );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        return SetDefaultDisposalStrategy( strategy );
    }

    IDependencyLocatorBuilder IDependencyLocatorBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        return ReinterpretCast.To<IDependencyContainerBuilder>( this ).SetDefaultDisposalStrategy( strategy );
    }
}
