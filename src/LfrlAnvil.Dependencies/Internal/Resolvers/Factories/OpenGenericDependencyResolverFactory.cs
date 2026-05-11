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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class OpenGenericDependencyResolverFactory : RegisteredDependencyResolverFactory
{
    internal OpenGenericDependencyResolverFactory(
        ImplementorKey implementorKey,
        DependencyLifetime lifetime,
        IOpenGenericDependencyImplementorBuilder implementorBuilder,
        bool isLastRangeElement)
        : base( implementorKey, lifetime, isOpenGeneric: true )
    {
        Assume.True( ! isLastRangeElement || implementorKey.RangeIndex is null );
        ImplementorBuilder = implementorBuilder;
        IsLastRangeElement = isLastRangeElement;
    }

    internal IOpenGenericDependencyImplementorBuilder ImplementorBuilder { get; }
    internal bool IsLastRangeElement { get; }

    internal override DependencyResolverFactory Close(
        IInternalDependencyKey dependencyKey,
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories)
    {
        Assume.False( dependencyKey.Type.ContainsGenericParameters );

        if ( ImplementorKey.IsShared )
        {
            var sharedResolverFactory = CloseShared( dependencyKey, in @params );
            if ( IsLastRangeElement )
            {
                if ( ReferenceEquals( dynamicResolverFactories, @params.ResolverFactories ) )
                    dynamicResolverFactories[dependencyKey] = sharedResolverFactory;
                else
                    dynamicResolverFactories.Add( dependencyKey, sharedResolverFactory );
            }

            return sharedResolverFactory;
        }

        Assume.True( Equals( ImplementorKey.Value.Key, dependencyKey.Key ) );
        if ( ImplementorKey.RangeIndex is not null )
            return RegisteredClosedGenericDependencyResolverFactory.Create(
                this,
                ImplementorKey.Create( dependencyKey, ImplementorKey.RangeIndex ) );

        ref var closedFactory = ref CollectionsMarshal.GetValueRefOrAddDefault( dynamicResolverFactories, dependencyKey, out var exists )!;
        if ( ! exists )
            closedFactory = RegisteredClosedGenericDependencyResolverFactory.Create( this, ImplementorKey.Create( dependencyKey ) );

        return closedFactory;
    }

    internal DependencyResolverFactory CloseShared(
        IInternalDependencyKey dependencyKey,
        in DependencyLocatorBuilderExtractionParams @params)
    {
        return CloseShared( dependencyKey, ImplementorKey.Value.Type, in @params );
    }

    internal DependencyResolverFactory CloseShared(
        IInternalDependencyKey dependencyKey,
        Type implementorType,
        in DependencyLocatorBuilderExtractionParams @params)
    {
        Assume.False( dependencyKey.Type.ContainsGenericParameters );
        Assume.True( ImplementorKey.IsShared );

        var sharedImplementorType = implementorType.CloseImplementorType( dependencyKey.Type );
        var sharedImplementorKey = InternalImplementorKey.WithType( sharedImplementorType );
        var sharedResolverFactory = @params.GetSharedResolverFactory( sharedImplementorKey, Lifetime );
        if ( sharedResolverFactory is null )
        {
            sharedResolverFactory = RegisteredClosedGenericDependencyResolverFactory.Create(
                this,
                ImplementorKey.CreateShared( sharedImplementorKey ) );

            @params.AddSharedResolverFactory( sharedImplementorKey, Lifetime, sharedResolverFactory );
        }

        return sharedResolverFactory;
    }

    protected override bool TryResolveCreationMethodImmediately(
        UlongSequenceGenerator idGenerator,
        Dictionary<Type, Func<Type, object, IInternalDependencyKey>> typeErasedKeyFactories,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration,
        out DependencyResolver? resolver)
    {
        resolver = null;
        return true;
    }

    [Pure]
    protected override ConstructorInfo? FindValidConstructor(
        Dictionary<Type, Func<Type, object, IInternalDependencyKey>> typeErasedKeyFactories,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration)
    {
        var ctor = ImplementorBuilder.Constructor?.Info;

        if ( ctor is not null )
        {
            var declaringType = ctor.DeclaringType;
            if ( declaringType is null || ! declaringType.IsOpenGenericAssignableTo( ImplementorKey.Value.Type ) )
                Errors = Errors.Extend( Resources.ProvidedConstructorDoesNotCreateInstancesOfCorrectType( ctor ) );

            if ( declaringType?.IsAbstract != false )
                Errors = Errors.Extend( Resources.ProvidedConstructorBelongsToNonConstructableType( ctor ) );

            return Errors.Count == 0 ? ctor : null;
        }

        Type type;
        var explicitType = ImplementorBuilder.Constructor?.Type;
        if ( explicitType is not null )
        {
            type = explicitType;
            if ( ! explicitType.IsOpenGenericAssignableTo( ImplementorKey.Value.Type ) )
                Errors = Errors.Extend( Resources.ProvidedTypeIsIncorrect( explicitType ) );
        }
        else
            type = ImplementorKey.Value.Type;

        if ( type.IsAbstract )
            Errors = Errors.Extend( Resources.ProvidedTypeIsNonConstructable( explicitType ) );

        if ( Errors.Count == 0 )
        {
            ctor = FindBestSuitedCtor( type, typeErasedKeyFactories, availableDependencies, configuration );
            if ( ctor is null )
                Errors = Errors.Extend( Resources.FailedToFindValidCtorForType( explicitType ) );
        }

        return ctor;
    }

    protected override bool ValidateDependencies(
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
                ParameterResolutions[i] = KeyValuePair.Create(
                    parameter,
                    ( object? )CustomOpenGenericResolutionFactory.TryCreate(
                        parameterFactory,
                        implementorKey.Type,
                        customResolutionIndex >= 0 ) );

                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                continue;
            }

            var openImplementorType = implementorKey.Type.GetOpenGenericDependencyType();
            if ( openImplementorType is not null && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( openImplementorType );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out parameterFactory ) && parameterFactory.IsOpenGeneric )
                {
                    if ( ! implementorKey.Type.ContainsGenericParameters )
                        parameterFactory = parameterFactory.Close( internalKey, in @params, dynamicResolverFactories );

                    ParameterResolutions[i] = KeyValuePair.Create(
                        parameter,
                        ( object? )CustomOpenGenericResolutionFactory.TryCreate(
                            parameterFactory,
                            implementorKey.Type,
                            customResolutionIndex >= 0 ) );

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
                MemberResolutions[i] = KeyValuePair.Create(
                    member,
                    ( object? )CustomOpenGenericResolutionFactory.TryCreate(
                        memberFactory,
                        implementorKey.Type,
                        customResolutionIndex >= 0 ) );

                captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                continue;
            }

            var openImplementorType = implementorKey.Type.GetOpenGenericDependencyType();
            if ( openImplementorType is not null && implementorKey is IInternalDependencyKey internalKey )
            {
                var openGenericKey = internalKey.WithType( openImplementorType );
                if ( @params.ResolverFactories.TryGetValue( openGenericKey, out memberFactory ) && memberFactory.IsOpenGeneric )
                {
                    if ( ! implementorKey.Type.ContainsGenericParameters )
                        memberFactory = memberFactory.Close( internalKey, in @params, dynamicResolverFactories );

                    MemberResolutions[i] = KeyValuePair.Create(
                        member,
                        ( object? )CustomOpenGenericResolutionFactory.TryCreate(
                            memberFactory,
                            implementorKey.Type,
                            customResolutionIndex >= 0 ) );

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

    protected override DependencyResolver CreateResolver(
        UlongSequenceGenerator idGenerator,
        DependencyContainerConfigurationBuilder configuration)
    {
        Assume.IsNotNull( ConstructorInfo );

        object?[]? parameterResolvers = null;
        KeyValuePair<MemberInfo, object?>[]? memberResolvers = null;

        if ( ParameterResolutions is not null )
        {
            Assume.ContainsAtLeast( ParameterResolutions, 1 );
            parameterResolvers = new object?[ParameterResolutions.Length];
            for ( var i = 0; i < parameterResolvers.Length; ++i )
            {
                var resolution = ParameterResolutions[i].Value;
                parameterResolvers[i] = resolution is null or LambdaExpression
                    ? resolution
                    : CustomOpenGenericResolution.TryCreate( idGenerator, configuration, resolution );
            }
        }

        if ( MemberResolutions is not null )
        {
            Assume.ContainsAtLeast( MemberResolutions, 1 );
            memberResolvers = new KeyValuePair<MemberInfo, object?>[MemberResolutions.Length];
            for ( var i = 0; i < memberResolvers.Length; ++i )
            {
                var resolution = MemberResolutions[i];
                memberResolvers[i] = KeyValuePair.Create(
                    resolution.Key,
                    resolution.Value is null or LambdaExpression
                        ? resolution.Value
                        : CustomOpenGenericResolution.TryCreate( idGenerator, configuration, resolution.Value ) );
            }
        }

        return new OpenGenericDependencyResolver(
            idGenerator.Generate(),
            ImplementorBuilder.ImplementorType,
            new ResolvedInstanceDisposalStrategy( ImplementorBuilder.DisposalStrategy, ConstructorInfo ),
            ConstructorInfo,
            parameterResolvers,
            memberResolvers,
            ImplementorBuilder.OnResolvingCallback,
            ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback,
            configuration.InjectablePropertyType,
            ReinterpretCast.To<IInternalDependencyKey>( ImplementorKey.Value ),
            ImplementorKey.IsShared,
            ! IsLastRangeElement,
            Lifetime );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private IDependencyKey ValidateDependencyImplementorType<T>(T target, Type dependencyType, IDependencyKey? implementorKey)
        where T : notnull
    {
        Assume.IsNotNull( implementorKey );

        if ( ! dependencyType.IsAnyResolvableBy( implementorKey.Type ) )
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
                    if ( ! parameter.ParameterType.IsAnyResolvableBy( resolution.ImplementorKey.Type ) )
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
