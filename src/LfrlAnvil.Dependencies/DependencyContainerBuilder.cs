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

    Type? IDependencyLocatorBuilder.KeyType => ((IDependencyLocatorBuilder)_locatorBuilderStore.Global).KeyType;
    object? IDependencyLocatorBuilder.Key => ((IDependencyLocatorBuilder)_locatorBuilderStore.Global).Key;
    bool IDependencyLocatorBuilder.IsKeyed => ((IDependencyLocatorBuilder)_locatorBuilderStore.Global).IsKeyed;

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
    public IDependencyBuilder? TryGetDependency(Type type)
    {
        return _locatorBuilderStore.Global.TryGetDependency( type );
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
        {
            if ( factory.IsInternal )
                continue;

            ReinterpretCast.To<ImplementorBasedDependencyResolverFactory>( factory )
                .PrepareCreationMethod( idGenerator, extractionParams.ResolverFactories, Configuration );
        }

        foreach ( var factory in resolverFactories )
        {
            if ( factory.IsInternal )
                continue;

            ReinterpretCast.To<ImplementorBasedDependencyResolverFactory>( factory )
                .ValidateRequiredDependencies( extractionParams.ResolverFactories, Configuration );
        }

        var pathBuffer = new List<(object?, ImplementorBasedDependencyResolverFactory)>();
        foreach ( var factory in resolverFactories )
        {
            if ( factory.IsInternal )
                continue;

            ReinterpretCast.To<ImplementorBasedDependencyResolverFactory>( factory )
                .ValidateCircularDependencies( pathBuffer );
        }

        var handledImplementorMessages = new HashSet<ImplementorKey>();
        foreach ( var factory in resolverFactories )
        {
            var factoryMessages = factory.GetMessages();
            if ( factoryMessages is not null && handledImplementorMessages.Add( factoryMessages.Value.ImplementorKey ) )
                messages = messages.Extend( factoryMessages.Value );
        }

        foreach ( var message in messages )
        {
            if ( message.Errors.Count > 0 )
                return new DependencyContainerBuildResult<DependencyContainer>( null, messages );
        }

        foreach ( var factory in resolverFactories )
        {
            if ( ! factory.IsInternal )
                ReinterpretCast.To<ImplementorBasedDependencyResolverFactory>( factory ).Build( idGenerator );
        }

        var defaultResolvers = extractionParams.GetDefaultResolvers();
        var globalDependencyResolvers = new Dictionary<Type, DependencyResolver>( defaultResolvers );
        var keyedDependencyResolvers = KeyedDependencyResolversStore.Create( defaultResolvers );

        foreach ( var (dependencyKey, factory) in extractionParams.ResolverFactories )
        {
            if ( factory.IsInternal )
                continue;

            var resolvers = ReinterpretCast.To<IInternalDependencyKey>( dependencyKey )
                .GetTargetResolvers( globalDependencyResolvers, keyedDependencyResolvers );

            resolvers.Add( dependencyKey.Type, factory.GetResolver() );
        }

        var result = new DependencyContainer( globalDependencyResolvers, keyedDependencyResolvers );
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
