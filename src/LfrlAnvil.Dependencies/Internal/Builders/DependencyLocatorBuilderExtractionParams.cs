// Copyright 2024 Łukasz Furlepa
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
