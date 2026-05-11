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
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal readonly struct DependencyLocatorBuilderExtractionParams
{
    internal readonly Dictionary<IDependencyKey, DependencyResolverFactory> ResolverFactories;
    internal readonly Dictionary<Type, Func<Type, object, IInternalDependencyKey>> TypeErasedKeyFactories;

    private readonly List<(Type DependencyType, DependencyResolverFactory Factory)> _defaultResolverFactories;
    private readonly Dictionary<(IDependencyKey, DependencyLifetime), DependencyResolverFactory> _sharedResolvers;

    private DependencyLocatorBuilderExtractionParams(Dictionary<IDependencyKey, DependencyResolverFactory> resolverFactories)
    {
        _sharedResolvers = new Dictionary<(IDependencyKey, DependencyLifetime), DependencyResolverFactory>();
        _defaultResolverFactories = new List<(Type, DependencyResolverFactory)>( capacity: 3 );
        ResolverFactories = resolverFactories;
        TypeErasedKeyFactories = new Dictionary<Type, Func<Type, object, IInternalDependencyKey>>();
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
    internal void RegisterCustomDefaultResolverFactory(Type dependencyType, DependencyResolverFactory factory)
    {
        _defaultResolverFactories.Add( (dependencyType, factory) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void FinalizeDefaultResolverFactories(UlongSequenceGenerator idGenerator)
    {
        _defaultResolverFactories.Add(
            (typeof( IDependencyContainer ),
                DependencyResolverFactory.CreateFinished(
                    ImplementorKey.CreateShared( new DependencyKey( typeof( IDependencyContainer ) ) ),
                    DependencyLifetime.Singleton,
                    new DependencyContainerResolver( idGenerator.Generate() ) )) );

        _defaultResolverFactories.Add(
            (typeof( IDependencyScope ),
                DependencyResolverFactory.CreateFinished(
                    ImplementorKey.CreateShared( new DependencyKey( typeof( IDependencyScope ) ) ),
                    DependencyLifetime.Singleton,
                    new DependencyScopeResolver( idGenerator.Generate() ) )) );

        _defaultResolverFactories.Add(
            (typeof( IDependencyScopeFactory ),
                DependencyResolverFactory.CreateFinished(
                    ImplementorKey.CreateShared( new DependencyKey( typeof( IDependencyScopeFactory ) ) ),
                    DependencyLifetime.Singleton,
                    new DependencyScopeFactoryResolver( idGenerator.Generate() ) )) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddDefaultResolverFactories(DependencyLocatorBuilder locatorBuilder)
    {
        foreach ( var (dependencyType, factory) in _defaultResolverFactories )
            ResolverFactories[locatorBuilder.CreateImplementorKey( dependencyType )] = factory;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReadOnlyArray<KeyValuePair<Type, DependencyResolver>> GetDefaultResolvers()
    {
        var result = new Dictionary<Type, DependencyResolver>();
        foreach ( var (dependencyType, factory) in _defaultResolverFactories )
            result[dependencyType] = factory.GetResolver();

        return result.ToArray();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorBuilderExtractionParams Create()
    {
        return new DependencyLocatorBuilderExtractionParams( new Dictionary<IDependencyKey, DependencyResolverFactory>() );
    }
}
