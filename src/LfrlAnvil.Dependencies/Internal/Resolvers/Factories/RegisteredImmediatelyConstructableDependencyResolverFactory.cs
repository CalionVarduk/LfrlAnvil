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

    internal ResolvedInstanceDisposalStrategy DisposalStrategy => new ResolvedInstanceDisposalStrategy(
        ImplementorBuilder.DisposalStrategy,
        ConstructorInfo );

    protected override Action<object, Type, IDependencyScope>? OnCreatedCallback =>
        ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback;

    protected sealed override bool TryResolveCreationMethodImmediately(
        UlongSequenceGenerator idGenerator,
        Dictionary<Type, Func<Type, object, IInternalDependencyKey>> typeErasedKeyFactories,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration,
        out DependencyResolver? resolver)
    {
        resolver = ImplementorBuilder.Factory is not null ? CreateFromFactory( idGenerator ) : null;
        return true;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected sealed override ConstructorInfo? FindValidConstructor(
        Dictionary<Type, Func<Type, object, IInternalDependencyKey>> typeErasedKeyFactories,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration)
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
            ctor = FindBestSuitedCtor( type, typeErasedKeyFactories, availableDependencies, configuration );
            if ( ctor is null )
                Errors = Errors.Extend( Resources.FailedToFindValidCtorForType( explicitType ) );
        }

        return ctor;
    }

    protected sealed override bool ValidateDependencies(
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        DependencyContainerConfigurationBuilder configuration,
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
            {
                var customKey = configuration.ConstructorParameterKeyProvider?.Invoke( parameter );
                implementorKey = customKey is null
                    ? InternalImplementorKey.WithType( parameter.ParameterType )
                    : DependencyKey.CreateKeyedTypeErased( @params.TypeErasedKeyFactories, parameter.ParameterType, customKey );
            }
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

            var openImplementorType = implementorKey.Type.GetOpenGenericDependencyType();
            if ( openImplementorType is not null && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( openImplementorType );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out parameterFactory ) && parameterFactory.IsOpenGeneric )
                {
                    parameterFactory = parameterFactory.Close( internalKey, in @params, dynamicResolverFactories );
                    ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )parameterFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                    continue;
                }
            }

            if ( parameter.IsInjectableParameterOptional( configuration ) )
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
            {
                var customKey = configuration.MemberKeyProvider?.Invoke( member.GetActualMember() );
                implementorKey = customKey is null
                    ? InternalImplementorKey.WithType( memberType )
                    : DependencyKey.CreateKeyedTypeErased( @params.TypeErasedKeyFactories, memberType, customKey );
            }
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

            var openImplementorType = implementorKey.Type.GetOpenGenericDependencyType();
            if ( openImplementorType is not null && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( openImplementorType );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out memberFactory ) && memberFactory.IsOpenGeneric )
                {
                    memberFactory = memberFactory.Close( internalKey, in @params, dynamicResolverFactories );
                    MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                    captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                    continue;
                }
            }

            if ( member.IsInjectableMemberOptional( memberType, configuration ) )
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

    private ConstructorInfo? FindBestSuitedCtor(
        Type type,
        Dictionary<Type, Func<Type, object, IInternalDependencyKey>> typeErasedKeyFactories,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration)
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
                {
                    var customKey = configuration.ConstructorParameterKeyProvider?.Invoke( parameter );
                    implementorKey = customKey is null
                        ? InternalImplementorKey.WithType( parameter.ParameterType )
                        : DependencyKey.CreateKeyedTypeErased( typeErasedKeyFactories, parameter.ParameterType, customKey );
                }
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

                var openImplementorType = implementorKey.Type.GetOpenGenericDependencyType();
                if ( openImplementorType is not null && implementorKey is IInternalDependencyKey internalKey )
                {
                    var openGenericKey = internalKey.WithType( openImplementorType );
                    if ( availableDependencies.TryGetValue( openGenericKey, out parameterFactory ) && parameterFactory.IsOpenGeneric )
                    {
                        if ( ! parameterFactory.IsCaptiveDependencyOf( Lifetime ) )
                            score += defaultScore * 2;

                        continue;
                    }
                }

                if ( ! parameter.IsInjectableParameterOptional( configuration ) )
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
