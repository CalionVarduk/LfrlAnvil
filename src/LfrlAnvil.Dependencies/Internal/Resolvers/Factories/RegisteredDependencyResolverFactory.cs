// Copyright 2026 Łukasz Furlepa
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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RegisteredDependencyResolverFactory : DependencyResolverFactory
{
    protected RegisteredDependencyResolverFactory(ImplementorKey implementorKey, DependencyLifetime lifetime, bool isOpenGeneric)
        : base( implementorKey, lifetime, isOpenGeneric )
    {
        ConstructorInfo = null;
        ParameterResolutions = null;
        MemberResolutions = null;
        Errors = Chain<string>.Empty;
        Warnings = Chain<string>.Empty;
    }

    internal ConstructorInfo? ConstructorInfo { get; private set; }
    internal KeyValuePair<ParameterInfo, object?>[]? ParameterResolutions { get; set; }
    internal KeyValuePair<MemberInfo, object?>[]? MemberResolutions { get; set; }
    protected Chain<string> Errors { get; set; }
    protected Chain<string> Warnings { get; set; }

    [Pure]
    internal sealed override Chain<DependencyResolverFactory> GetCaptiveDependencyFactories(DependencyLifetime lifetime)
    {
        return IsCaptiveDependencyOf( lifetime ) ? Chain.Create<DependencyResolverFactory>( this ) : Chain<DependencyResolverFactory>.Empty;
    }

    [Pure]
    internal sealed override bool IsCaptiveDependencyOf(DependencyLifetime lifetime)
    {
        return Lifetime < lifetime;
    }

    [Pure]
    protected sealed override Chain<DependencyContainerBuildMessages> CreateMessages()
    {
        return Errors.Count == 0 && Warnings.Count == 0
            ? Chain<DependencyContainerBuildMessages>.Empty
            : Chain.Create( new DependencyContainerBuildMessages( ImplementorKey, Errors, Warnings ) );
    }

    protected sealed override bool IsCreationMethodValid(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        if ( ! TryResolveCreationMethodImmediately( idGenerator, availableDependencies, configuration, out var resolver ) )
            return false;

        if ( resolver is not null )
        {
            Finish( resolver );
            return true;
        }

        ConstructorInfo = FindValidConstructor( availableDependencies, configuration );
        return ConstructorInfo is not null;
    }

    protected sealed override bool AreRequiredDependenciesValid(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration)
    {
        var captiveDependencies = Chain<string>.Empty;
        if ( ! ValidateDependencies( @params, dynamicResolverFactories, configuration, ref captiveDependencies ) )
            return false;

        if ( configuration.TreatCaptiveDependenciesAsErrors )
            Errors = Errors.Extend( captiveDependencies );
        else
            Warnings = Warnings.Extend( captiveDependencies );

        if ( Errors.Count > 0 )
        {
            ParameterResolutions = null;
            MemberResolutions = null;
            return false;
        }

        return true;
    }

    protected sealed override void OnCircularDependencyDetected(List<DependencyGraphNode> path)
    {
        var pathSpan = CollectionsMarshal.AsSpan( path );

        var startIndex = pathSpan.Length - 2;
        while ( ! ReferenceEquals( pathSpan[startIndex].Factory, this ) )
            --startIndex;

        pathSpan = pathSpan.Slice( startIndex + 1 );

        foreach ( var pathNode in pathSpan )
            AddState( pathNode.Factory, DependencyResolverFactoryState.CircularDependenciesDetected );

        Errors = Errors.Extend( Resources.CircularDependenciesDetected( pathSpan ) );
    }

    protected sealed override void DetectCircularDependenciesInChildren(List<DependencyGraphNode> path)
    {
        Assume.ContainsAtLeast( path, 1 );

        if ( ParameterResolutions is not null )
        {
            foreach ( var (parameter, resolution) in ParameterResolutions )
            {
                if ( resolution is not DependencyResolverFactory factory )
                    continue;

                path[^1] = new DependencyGraphNode( parameter, factory );
                DetectCircularDependencies( factory, path );
            }
        }

        if ( MemberResolutions is not null )
        {
            foreach ( var (member, resolution) in MemberResolutions )
            {
                if ( resolution is not DependencyResolverFactory factory )
                    continue;

                path[^1] = new DependencyGraphNode( member.GetActualMember(), factory );
                DetectCircularDependencies( factory, path );
            }
        }
    }

    protected abstract bool TryResolveCreationMethodImmediately(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration,
        out DependencyResolver? resolver);

    [Pure]
    protected abstract ConstructorInfo? FindValidConstructor(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration);

    protected abstract bool ValidateDependencies(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration,
        ref Chain<string> captiveDependencies);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected Chain<string> ValidateCaptiveDependency<T>(
        Chain<string> currentMessages,
        T target,
        IDependencyKey implementorKey,
        DependencyResolverFactory resolverFactory)
        where T : notnull
    {
        var captiveFactories = resolverFactory.GetCaptiveDependencyFactories( Lifetime );
        foreach ( var f in captiveFactories )
        {
            var message = Resources.CaptiveDependencyDetected( target, Lifetime, implementorKey, f.Lifetime, f.ImplementorKey.RangeIndex );
            currentMessages = currentMessages.Extend( message );
        }

        return currentMessages;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static BitArray? ReuseBitArray(BitArray? current, int expectedLength)
    {
        if ( expectedLength == 0 )
            return null;

        if ( current is null )
            return new BitArray( expectedLength );

        current.Length = expectedLength;
        current.SetAll( false );
        return current;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected static ConstructorInfo? FindBestScoredConstructor(ReadOnlyArray<ScoredConstructor> constructors)
    {
        ScoredConstructor? result = null;
        foreach ( var other in constructors )
        {
            if ( other.Score < 0 )
                continue;

            if ( result is null
                || other.Score > result.Value.Score
                || (other.Score == result.Value.Score && other.ParameterCount > result.Value.ParameterCount) )
                result = other;
        }

        return result?.Info;
    }

    protected readonly record struct ScoredConstructor(ConstructorInfo Info, int Score, int ParameterCount);
}
