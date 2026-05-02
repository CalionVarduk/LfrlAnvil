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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RegisteredImmediatelyConstructableDependencyResolverFactory : RegisteredConstructableDependencyResolverFactory
{
    protected RegisteredImmediatelyConstructableDependencyResolverFactory(
        ImplementorKey implementorKey,
        IDependencyImplementorBuilder implementorBuilder,
        DependencyLifetime lifetime)
        : base( implementorKey, lifetime )
    {
        ImplementorBuilder = implementorBuilder;
    }

    internal IDependencyImplementorBuilder ImplementorBuilder { get; }

    protected override Action<object, Type, IDependencyScope>? OnCreatedCallback =>
        ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback;

    protected sealed override bool TryResolveCreationMethodImmediately(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration,
        out DependencyResolver? resolver)
    {
        resolver = ImplementorBuilder.Factory is not null ? CreateFromFactory( idGenerator ) : null;
        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected sealed override ConstructorInfo? FindValidConstructor(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNull( ImplementorBuilder.Factory );

        var ctor = ImplementorBuilder.Constructor?.Info;
        if ( ctor is not null )
        {
            if ( ctor.DeclaringType?.IsAssignableTo( ImplementorKey.Value.Type ) != true )
                Errors = Errors.Extend( Resources.ProvidedConstructorDoesNotCreateInstancesOfCorrectType( ctor ) );

            if ( ctor.DeclaringType?.IsConstructable() != true )
                Errors = Errors.Extend( Resources.ProvidedConstructorBelongsToNonConstructableType( ctor ) );

            return Errors.Count == 0 ? ctor : null;
        }

        Type type;
        var explicitType = ImplementorBuilder.Constructor?.Type;
        if ( explicitType is not null )
        {
            type = explicitType;
            if ( ! explicitType.IsAssignableTo( ImplementorKey.Value.Type ) )
                Errors = Errors.Extend( Resources.ProvidedTypeIsIncorrect( explicitType ) );
        }
        else
            type = ImplementorKey.Value.Type;

        if ( ! type.IsConstructable() )
            Errors = Errors.Extend( Resources.ProvidedTypeIsNonConstructable( explicitType ) );

        if ( Errors.Count == 0 )
        {
            ctor = FindBestSuitedCtor( type, availableDependencies, configuration );
            if ( ctor is null )
                Errors = Errors.Extend( Resources.FailedToFindValidCtorForType( explicitType ) );
        }

        return ctor;
    }

    protected sealed override bool ValidateDependencies(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration,
        ref Chain<string> captiveDependencies)
    {
        Assume.IsNotNull( ConstructorInfo );
        var invocationOptions = ImplementorBuilder.Constructor?.InvocationOptions;

        var parameters = ConstructorInfo.GetParameters();
        var explicitParameterResolutions = invocationOptions?.ParameterResolutions;
        var explicitResolutionsLength = explicitParameterResolutions?.Count ?? 0;
        var usedExplicitResolutions = explicitResolutionsLength > 0 ? new BitArray( explicitResolutionsLength ) : null;

        if ( parameters.Length > 0 )
            ParameterResolutions = new KeyValuePair<ParameterInfo, object?>[parameters.Length];

        for ( var i = 0; i < parameters.Length; ++i )
        {
            Assume.IsNotNull( ParameterResolutions );

            var parameter = parameters[i];
            var customResolutionIndex = FindCustomResolutionIndex( explicitParameterResolutions, explicitResolutionsLength, parameter );

            IDependencyKey implementorKey;
            if ( customResolutionIndex == -1 )
                implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
            else
            {
                var resolution = GetResolution( explicitParameterResolutions, usedExplicitResolutions, customResolutionIndex );
                if ( resolution.Factory is not null )
                {
                    ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )resolution.Factory );
                    continue;
                }

                implementorKey = ValidateDependencyImplementorType( parameter, parameter.ParameterType, resolution.ImplementorKey );
            }

            if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
            {
                ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )parameterFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                continue;
            }

            if ( implementorKey.Type.IsGenericType && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( implementorKey.Type.GetGenericTypeDefinition() );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out parameterFactory )
                    && parameterFactory is OpenGenericDependencyResolverFactory genericParameterFactory )
                {
                    parameterFactory = genericParameterFactory.Close( internalKey, @params, dynamicResolverFactories );
                    ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )parameterFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                    continue;
                }
            }

            if ( parameter.HasDefaultValue
                || parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false )
                || (parameter.ParameterType.IsGenericType
                    && parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
            {
                ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )null );
                continue;
            }

            Errors = Errors.Extend( Resources.RequiredDependencyCannotBeResolved( parameter, implementorKey ) );
        }

        ValidateUnusedResolutions( explicitParameterResolutions, usedExplicitResolutions, explicitResolutionsLength );

        var injectableMembers = ConstructorInfo.DeclaringType?.FindInjectableMembers( configuration.InjectablePropertyType ) ?? [ ];
        var explicitMemberResolutions = invocationOptions?.MemberResolutions;
        explicitResolutionsLength = explicitMemberResolutions?.Count ?? 0;
        usedExplicitResolutions = ReuseBitArray( usedExplicitResolutions, explicitResolutionsLength );

        if ( injectableMembers.Count > 0 )
            MemberResolutions = new KeyValuePair<MemberInfo, object?>[injectableMembers.Count];

        for ( var i = 0; i < injectableMembers.Count; ++i )
        {
            Assume.IsNotNull( MemberResolutions );

            var member = injectableMembers[i];
            var customResolutionIndex = FindCustomResolutionIndex( explicitMemberResolutions, explicitResolutionsLength, member );

            var memberInjectableType = member.GetInjectableMemberType();
            var memberType = memberInjectableType.GetGenericArguments()[0];

            IDependencyKey implementorKey;
            if ( customResolutionIndex == -1 )
                implementorKey = InternalImplementorKey.WithType( memberType );
            else
            {
                var resolution = GetResolution( explicitMemberResolutions, usedExplicitResolutions, customResolutionIndex );
                if ( resolution.Factory is not null )
                {
                    MemberResolutions[i] = KeyValuePair.Create( member, ( object? )resolution.Factory );
                    continue;
                }

                implementorKey = ValidateDependencyImplementorType( member, memberType, resolution.ImplementorKey );
            }

            if ( @params.ResolverFactories.TryGetValue( implementorKey, out var memberFactory ) )
            {
                MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                continue;
            }

            if ( implementorKey.Type.IsGenericType && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( implementorKey.Type.GetGenericTypeDefinition() );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out memberFactory )
                    && memberFactory is OpenGenericDependencyResolverFactory genericMemberFactory )
                {
                    memberFactory = genericMemberFactory.Close( internalKey, @params, dynamicResolverFactories );
                    MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                    continue;
                }
            }

            if ( member.IsInjectableMemberOptional( configuration )
                || (memberType.IsGenericType && memberType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
            {
                MemberResolutions[i] = KeyValuePair.Create( member, ( object? )null );
                continue;
            }

            Errors = Errors.Extend( Resources.RequiredDependencyCannotBeResolved( member, implementorKey ) );
        }

        ValidateUnusedResolutions( explicitMemberResolutions, usedExplicitResolutions, explicitResolutionsLength );
        return true;
    }

    protected abstract DependencyResolver CreateFromFactory(UlongSequenceGenerator idGenerator);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int FindCustomResolutionIndex<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        int resolutionsLength,
        T target)
        where T : class, ICustomAttributeProvider
    {
        for ( var i = 0; i < resolutionsLength; ++i )
        {
            Assume.IsNotNull( resolutions );
            if ( resolutions[i].Predicate( target ) )
                return i;
        }

        return -1;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static InjectableDependencyResolution<T> GetResolution<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        BitArray? usedResolutions,
        int index)
        where T : class, ICustomAttributeProvider
    {
        Assume.IsNotNull( resolutions );
        Assume.IsNotNull( usedResolutions );
        usedResolutions[index] = true;
        return resolutions[index];
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IDependencyKey ValidateDependencyImplementorType<T>(T target, Type dependencyType, IDependencyKey? implementorKey)
        where T : notnull
    {
        Assume.IsNotNull( implementorKey );

        if ( ! implementorKey.Type.IsAssignableTo( dependencyType ) )
        {
            var message = Resources.ProvidedImplementorTypeIsNotAssignableToDependencyType( target, implementorKey );
            Errors = Errors.Extend( message );
        }

        return implementorKey;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ValidateUnusedResolutions<T>(
        IReadOnlyList<InjectableDependencyResolution<T>>? resolutions,
        BitArray? usedResolutions,
        int resolutionsLength)
        where T : class, ICustomAttributeProvider
    {
        Assume.IsNotNull( ConstructorInfo );

        for ( var i = 0; i < resolutionsLength; ++i )
        {
            Assume.IsNotNull( resolutions );
            Assume.IsNotNull( usedResolutions );

            if ( usedResolutions[i] )
                continue;

            var resolution = resolutions[i];
            var message = Resources.UnusedResolution<T>( ConstructorInfo, i, resolution.ImplementorKey, resolution.Factory );
            Warnings = Warnings.Extend( message );
        }
    }

    private ConstructorInfo? FindBestSuitedCtor(
        Type type,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        const int notEligibleScore = -1;
        const int defaultScore = 1;

        var invocationOptions = ImplementorBuilder.Constructor?.InvocationOptions;
        var explicitParameterResolutions = invocationOptions?.ParameterResolutions;
        var explicitResolutionsLength = explicitParameterResolutions?.Count ?? 0;

        var constructors = type.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
        var scoredConstructors = new ScoredConstructor[constructors.Length];

        for ( var i = 0; i < constructors.Length; ++i )
        {
            var ctor = constructors[i];
            var parameters = ctor.GetParameters();
            var score = ctor.IsPublic ? defaultScore : 0;

            foreach ( var parameter in parameters )
            {
                var customResolutionIndex = FindCustomResolutionIndex( explicitParameterResolutions, explicitResolutionsLength, parameter );

                IDependencyKey implementorKey;
                if ( customResolutionIndex == -1 )
                    implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
                else
                {
                    Assume.IsNotNull( explicitParameterResolutions );
                    var resolution = explicitParameterResolutions[customResolutionIndex];
                    if ( resolution.Factory is not null )
                    {
                        score += defaultScore * 3;
                        continue;
                    }

                    Assume.IsNotNull( resolution.ImplementorKey );
                    if ( ! resolution.ImplementorKey.Type.IsAssignableTo( parameter.ParameterType ) )
                    {
                        score = notEligibleScore;
                        break;
                    }

                    score += defaultScore;
                    implementorKey = resolution.ImplementorKey;
                }

                if ( availableDependencies.TryGetValue( implementorKey, out var parameterFactory ) )
                {
                    if ( ! parameterFactory.IsCaptiveDependencyOf( Lifetime ) )
                        score += defaultScore * 2;

                    continue;
                }

                if ( implementorKey.Type.IsGenericType && implementorKey is IInternalDependencyKey internalKey )
                {
                    var openGenericKey = internalKey.WithType( implementorKey.Type.GetGenericTypeDefinition() );
                    if ( availableDependencies.TryGetValue( openGenericKey, out parameterFactory )
                        && parameterFactory is OpenGenericDependencyResolverFactory )
                    {
                        if ( ! parameterFactory.IsCaptiveDependencyOf( Lifetime ) )
                            score += defaultScore * 2;

                        continue;
                    }
                }

                if ( ! parameter.HasDefaultValue
                    && ! parameter.HasAttribute( configuration.OptionalDependencyAttributeType, inherit: false )
                    && ! (parameter.ParameterType.IsGenericType
                        && parameter.ParameterType.GetGenericTypeDefinition() == typeof( IEnumerable<> )) )
                {
                    score = notEligibleScore;
                    break;
                }

                score += defaultScore;
            }

            scoredConstructors[i] = new ScoredConstructor( ctor, score, parameters.Length );
        }

        return FindBestScoredConstructor( scoredConstructors );
    }
}
