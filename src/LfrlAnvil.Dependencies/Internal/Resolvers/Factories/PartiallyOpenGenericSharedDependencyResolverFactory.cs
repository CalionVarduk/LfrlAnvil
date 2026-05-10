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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class PartiallyOpenGenericSharedDependencyResolverFactory : RegisteredDependencyResolverFactory
{
    internal PartiallyOpenGenericSharedDependencyResolverFactory(
        IDependencyKey implementorKey,
        OpenGenericDependencyResolverFactory @base)
        : base( ImplementorKey.CreateShared( implementorKey ), @base.Lifetime, isOpenGeneric: true )
    {
        Assume.True( @base.ImplementorKey.IsShared );
        Assume.True(
            implementorKey.Type.IsGenericType && implementorKey.Type.GetGenericTypeDefinition() == @base.ImplementorKey.Value.Type );

        Base = @base;
    }

    internal OpenGenericDependencyResolverFactory Base { get; }

    internal override DependencyResolverFactory Close(
        IInternalDependencyKey dependencyKey,
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories)
    {
        Assume.False( dependencyKey.Type.ContainsGenericParameters );

        var sharedResolverFactory = Base.CloseShared( dependencyKey, ImplementorKey.Value.Type, in @params );
        if ( Base.IsLastRangeElement )
        {
            if ( ReferenceEquals( dynamicResolverFactories, @params.ResolverFactories ) )
                dynamicResolverFactories[dependencyKey] = sharedResolverFactory;
            else
                dynamicResolverFactories.Add( dependencyKey, sharedResolverFactory );
        }

        return sharedResolverFactory;
    }

    protected override bool TryResolveCreationMethodImmediately(
        UlongSequenceGenerator idGenerator,
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration,
        out DependencyResolver? resolver)
    {
        resolver = null;
        Base.PrepareCreationMethod( idGenerator, availableDependencies, configuration );
        return ! Base.HasState( DependencyResolverFactoryState.Invalid );
    }

    [Pure]
    protected override ConstructorInfo? FindValidConstructor(
        Dictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        DependencyContainerConfigurationBuilder configuration)
    {
        var openCtor = Base.ConstructorInfo;
        Assume.IsNotNull( openCtor );
        var result = openCtor.TryCloseGenericCtor( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type );
        if ( result is null )
            Errors = Errors.Extend(
                Resources.FailedToFindValidCtorForClosedGenericType( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type ) );

        return result;
    }

    protected override bool ValidateDependencies(
        in DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        DependencyContainerConfigurationBuilder configuration,
        ref Chain<string> captiveDependencies)
    {
        Base.ValidateRequiredDependencies( in @params, dynamicResolverFactories, configuration );
        if ( Base.HasState( DependencyResolverFactoryState.Invalid ) )
            return false;

        Assume.IsNotNull( ConstructorInfo );

        var parameters = ConstructorInfo.GetParameters();
        if ( parameters.Length > 0 )
        {
            Assume.IsNotNull( Base.ParameterResolutions );
            Assume.Equals( Base.ParameterResolutions.Length, parameters.Length );
            ParameterResolutions = new KeyValuePair<ParameterInfo, object?>[parameters.Length];

            for ( var i = 0; i < ParameterResolutions.Length; ++i )
            {
                var parameter = parameters[i];
                var baseResolution = Base.ParameterResolutions[i];
                if ( parameter.ParameterType.ContainsGenericParameters )
                {
                    ParameterResolutions[i] = KeyValuePair.Create( parameter, baseResolution.Value );
                    continue;
                }

                if ( baseResolution.Value is null )
                {
                    var implementorKey = InternalImplementorKey.WithType( parameter.ParameterType );
                    if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                    {
                        ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                        continue;
                    }

                    ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )null );
                }
                else if ( baseResolution.Value is LambdaExpression factory )
                    ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )factory );
                else
                {
                    var (baseResolver, implementorType) = CustomOpenGenericResolutionFactory.Extract( baseResolution.Value );
                    if ( ! baseResolver.IsOpenGeneric )
                        ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )baseResolver );
                    else
                    {
                        var implementorKey = InternalImplementorKey.WithType(
                            baseResolver is OpenGenericRangeDependencyResolverFactory
                                ? parameter.ParameterType
                                : implementorType.CloseImplementorType( parameter.ParameterType ) );

                        if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                        {
                            ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )parameterFactory );
                            captiveDependencies = ValidateCaptiveDependency(
                                captiveDependencies,
                                parameter,
                                implementorKey,
                                parameterFactory );

                            continue;
                        }

                        parameterFactory = baseResolver.Close( implementorKey, in @params, dynamicResolverFactories );
                        ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, parameter, implementorKey, parameterFactory );
                    }
                }
            }
        }

        if ( Base.MemberResolutions is not null )
        {
            var injectableMembers = ConstructorInfo.DeclaringType?.FindInjectableMembers( configuration.InjectablePropertyType ) ?? [ ];
            Assume.ContainsExactly( injectableMembers, Base.MemberResolutions.Length );
            MemberResolutions = new KeyValuePair<MemberInfo, object?>[injectableMembers.Count];

            for ( var i = 0; i < MemberResolutions.Length; ++i )
            {
                var member = injectableMembers[i];
                var memberInjectableType = member.GetInjectableMemberType();
                var memberType = memberInjectableType.GetGenericArguments()[0];
                var baseResolution = member.FindCorrespondingOpenTypeMemberResolution( Base.MemberResolutions );
                if ( memberType.ContainsGenericParameters )
                {
                    MemberResolutions[i] = KeyValuePair.Create( member, baseResolution );
                    continue;
                }

                if ( baseResolution is null )
                {
                    var implementorKey = InternalImplementorKey.WithType( memberType );
                    if ( @params.ResolverFactories.TryGetValue( implementorKey, out var parameterFactory ) )
                    {
                        MemberResolutions[i] = KeyValuePair.Create( member, ( object? )parameterFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, parameterFactory );
                        continue;
                    }

                    MemberResolutions[i] = KeyValuePair.Create( member, ( object? )null );
                }
                else if ( baseResolution is LambdaExpression factory )
                    MemberResolutions[i] = KeyValuePair.Create( member, ( object? )factory );
                else
                {
                    var (baseResolver, implementorType) = CustomOpenGenericResolutionFactory.Extract( baseResolution );
                    if ( ! baseResolver.IsOpenGeneric )
                        MemberResolutions[i] = KeyValuePair.Create( member, ( object? )baseResolver );
                    else
                    {
                        var implementorKey = InternalImplementorKey.WithType(
                            baseResolver is OpenGenericRangeDependencyResolverFactory
                                ? memberType
                                : implementorType.CloseImplementorType( memberType ) );

                        if ( @params.ResolverFactories.TryGetValue( implementorKey, out var memberFactory ) )
                        {
                            MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                            captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                            continue;
                        }

                        memberFactory = baseResolver.Close( implementorKey, in @params, dynamicResolverFactories );
                        MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                    }
                }
            }
        }

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
            ImplementorKey.Value.Type,
            Base.ImplementorBuilder.DisposalStrategy,
            ConstructorInfo,
            parameterResolvers,
            memberResolvers,
            Base.ImplementorBuilder.OnResolvingCallback,
            Base.ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback,
            configuration.InjectablePropertyType,
            ReinterpretCast.To<IInternalDependencyKey>( Base.ImplementorKey.Value ),
            isShared: true,
            ! Base.IsLastRangeElement,
            Lifetime );
    }
}
