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
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RangeDependencyResolverFactory : DependencyResolverFactory
{
    internal RangeDependencyResolverFactory(
        ImplementorKey implementorKey,
        Action<Type, IDependencyScope>? onResolvingCallback,
        DependencyResolverFactory[]? factories,
        bool isOpenGeneric)
        : base( implementorKey, DependencyLifetime.Transient, isOpenGeneric )
    {
        Assume.True( implementorKey.Value.Type.IsGenericType );
        Assume.Equals( implementorKey.Value.Type.GetGenericTypeDefinition(), typeof( IEnumerable<> ) );
        ElementType = implementorKey.Value.Type.GetGenericArguments()[0];
        OnResolvingCallback = onResolvingCallback;
        Factories = factories;
    }

    internal DependencyResolverFactory[]? Factories { get; }
    internal Type ElementType { get; }
    internal Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    [Pure]
    internal override Chain<DependencyResolverFactory> GetCaptiveDependencyFactories(DependencyLifetime lifetime)
    {
        var result = Chain<DependencyResolverFactory>.Empty;
        if ( Factories is null )
            return result;

        foreach ( var f in Factories )
            result = result.Extend( f.GetCaptiveDependencyFactories( lifetime ) );

        return result;
    }

    [Pure]
    internal override bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        if ( Factories is null )
            return false;

        foreach ( var f in Factories )
        {
            if ( f.IsCaptiveDependencyOf( lifetime ) )
                return true;
        }

        return false;
    }

    [Pure]
    protected override Chain<DependencyContainerBuildMessages> CreateMessages()
    {
        var result = Chain<DependencyContainerBuildMessages>.Empty;
        if ( Factories is null )
            return result;

        foreach ( var f in Factories )
            result = result.Extend( f.GetMessages() );

        return result;
    }

    protected override bool IsCreationMethodValid(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( Factories is null )
            return true;

        foreach ( var f in Factories )
            f.PrepareCreationMethod( idGenerator, availableDependencies, configuration );

        return true;
    }

    protected override bool AreRequiredDependenciesValid(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( Factories is null )
            return true;

        foreach ( var f in Factories )
            f.ValidateRequiredDependencies( @params, dynamicResolverFactories, configuration );

        return true;
    }

    protected override void OnCircularDependencyDetected(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 1 );
        Assume.IsNotNull( Factories );

        var reachedFrom = path[^1].ReachedFrom;
        path.Add( default );

        foreach ( var f in Factories )
        {
            path[^1] = new DependencyGraphNode( reachedFrom, f );
            DetectCircularDependencies( f, path );
        }

        path.RemoveLast();
    }

    protected override void DetectCircularDependenciesInChildren(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 2 );
        if ( Factories is null )
            return;

        var reachedFrom = path[^2].ReachedFrom;
        foreach ( var f in Factories )
        {
            path[^1] = new DependencyGraphNode( reachedFrom, f );
            DetectCircularDependencies( f, path );
        }
    }
}
