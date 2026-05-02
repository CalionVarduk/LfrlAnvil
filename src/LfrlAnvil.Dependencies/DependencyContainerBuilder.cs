// Copyright 2024-2026 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

/// <inheritdoc cref="IDependencyContainerBuilder" />
public class DependencyContainerBuilder : IDependencyContainerBuilder
{
    private readonly DependencyLocatorBuilderStore _locatorBuilderStore;

    /// <summary>
    /// Creates a new empty <see cref="DependencyContainerBuilder"/> instance.
    /// </summary>
    public DependencyContainerBuilder()
    {
        _locatorBuilderStore = DependencyLocatorBuilderStore.Create();
        Configuration = new DependencyContainerConfigurationBuilder();
    }

    /// <inheritdoc />
    public IDependencyContainerConfigurationBuilder Configuration { get; }

    /// <inheritdoc />
    public DependencyLifetime DefaultLifetime => _locatorBuilderStore.Global.DefaultLifetime;

    /// <inheritdoc />
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy => _locatorBuilderStore.Global.DefaultDisposalStrategy;

    Type? IDependencyLocatorBuilder.KeyType => (( IDependencyLocatorBuilder )_locatorBuilderStore.Global).KeyType;
    object? IDependencyLocatorBuilder.Key => (( IDependencyLocatorBuilder )_locatorBuilderStore.Global).Key;
    bool IDependencyLocatorBuilder.IsKeyed => (( IDependencyLocatorBuilder )_locatorBuilderStore.Global).IsKeyed;

    /// <inheritdoc />
    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        return _locatorBuilderStore.Global.AddSharedImplementor( type );
    }

    /// <inheritdoc />
    public IOpenGenericDependencyImplementorBuilder AddSharedGenericImplementor(Type type)
    {
        return _locatorBuilderStore.Global.AddSharedGenericImplementor( type );
    }

    /// <inheritdoc />
    public IDependencyBuilder Add(Type type)
    {
        return _locatorBuilderStore.Global.Add( type );
    }

    /// <inheritdoc />
    public IOpenGenericDependencyBuilder AddGeneric(Type type)
    {
        return _locatorBuilderStore.Global.AddGeneric( type );
    }

    /// <inheritdoc cref="IDependencyContainerBuilder.SetDefaultLifetime(DependencyLifetime)" />
    public DependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        _locatorBuilderStore.Global.SetDefaultLifetime( lifetime );
        return this;
    }

    /// <inheritdoc cref="IDependencyContainerBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy)" />
    public DependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        _locatorBuilderStore.Global.SetDefaultDisposalStrategy( strategy );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public IDependencyImplementorBuilder? TryGetSharedImplementor(Type type)
    {
        return _locatorBuilderStore.Global.TryGetSharedImplementor( type );
    }

    /// <inheritdoc />
    [Pure]
    public IOpenGenericDependencyImplementorBuilder? TryGetSharedGenericImplementor(Type type)
    {
        return _locatorBuilderStore.Global.TryGetSharedGenericImplementor( type );
    }

    /// <inheritdoc />
    [Pure]
    public IDependencyRangeBuilder GetDependencyRange(Type type)
    {
        return _locatorBuilderStore.Global.GetDependencyRange( type );
    }

    /// <inheritdoc />
    [Pure]
    public IOpenGenericDependencyRangeBuilder GetGenericDependencyRange(Type type)
    {
        return _locatorBuilderStore.Global.GetGenericDependencyRange( type );
    }

    /// <inheritdoc />
    public IDependencyLocatorBuilder<TKey> GetKeyedLocator<TKey>(TKey key)
        where TKey : notnull
    {
        return _locatorBuilderStore.GetOrAddKeyed( key );
    }

    /// <inheritdoc cref="IDependencyContainerBuilder.TryBuild()" />
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
        var resolverFactoriesToScan = resolverFactories;
        while ( resolverFactoriesToScan.Count > 0 )
        {
            var dynamicResolverFactories = new Dictionary<IDependencyKey, DependencyResolverFactory>();
            foreach ( var factory in resolverFactoriesToScan )
                factory.PrepareCreationMethod( idGenerator, extractionParams.ResolverFactories, Configuration );

            foreach ( var factory in resolverFactoriesToScan )
                factory.ValidateRequiredDependencies( extractionParams, dynamicResolverFactories, Configuration );

            foreach ( var (key, factory) in dynamicResolverFactories )
                extractionParams.ResolverFactories.Add( key, factory );

            resolverFactoriesToScan = dynamicResolverFactories.Values;
        }

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
            factory.Build( idGenerator, Configuration );

        var defaultResolvers = extractionParams.GetDefaultResolvers();
        var globalResolvers = DependencyResolversStore.Create( new Dictionary<Type, DependencyResolver>( defaultResolvers ) );
        var keyedDependencyResolvers = KeyedDependencyResolversStore.Create( defaultResolvers );

        foreach ( var (dependencyKey, factory) in extractionParams.ResolverFactories )
        {
            var dependencyStore = ReinterpretCast.To<IInternalDependencyKey>( dependencyKey )
                .GetTargetResolversStore( in globalResolvers, in keyedDependencyResolvers );

            var resolver = factory.GetResolver();
            dependencyStore.Resolvers.TryAdd( dependencyKey.Type, resolver );

            if ( factory.ImplementorKey.IsShared && factory is RegisteredClosedGenericDependencyResolverFactory closedGenericFactory )
            {
                var implementorStore = ReinterpretCast.To<IInternalDependencyKey>( closedGenericFactory.ImplementorKey.Value )
                    .GetTargetResolversStore( in globalResolvers, in keyedDependencyResolvers );

                implementorStore.SharedGenericResolvers.TryAdd(
                    new SharedGenericKey(
                        closedGenericFactory.Base.ImplementorKey.Value.Type,
                        closedGenericFactory.ImplementorKey.Value.Type ),
                    resolver );
            }
        }

        var result = new DependencyContainer( idGenerator, globalResolvers, keyedDependencyResolvers );
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
