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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Builders;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal abstract class RegisteredClosedGenericDependencyResolverFactory : RegisteredConstructableDependencyResolverFactory
{
    protected RegisteredClosedGenericDependencyResolverFactory(ImplementorKey implementorKey, OpenGenericDependencyResolverFactory @base)
        : base( implementorKey, @base.Lifetime )
    {
        Base = @base;
    }

    internal OpenGenericDependencyResolverFactory Base { get; }

    protected override Action<object, Type, IDependencyScope>? OnCreatedCallback =>
        Base.ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback;

    [Pure]
    internal static RegisteredClosedGenericDependencyResolverFactory Create(
        OpenGenericDependencyResolverFactory @base,
        ImplementorKey implementorKey)
    {
        Assume.True( implementorKey.Value.Type.IsGenericType && ! implementorKey.Value.Type.IsGenericTypeDefinition );
        Assume.True( @base.ImplementorKey.Value.Type.IsOpenGenericAssignableTo( implementorKey.Value.Type.GetGenericTypeDefinition() ) );

        RegisteredClosedGenericDependencyResolverFactory result = @base.Lifetime switch
        {
            DependencyLifetime.Singleton => new ClosedGenericSingletonDependencyResolverFactory( implementorKey, @base ),
            DependencyLifetime.ScopedSingleton => new ClosedGenericScopedSingletonDependencyResolverFactory( implementorKey, @base ),
            DependencyLifetime.Scoped => new ClosedGenericScopedDependencyResolverFactory( implementorKey, @base ),
            _ => new ClosedGenericTransientDependencyResolverFactory( implementorKey, @base )
        };

        return result;
    }

    internal sealed override void RegisterResolver(
        IDependencyKey dependencyKey,
        in DependencyResolversStore globalResolvers,
        in KeyedDependencyResolversStore keyedResolversStore)
    {
        base.RegisterResolver( dependencyKey, in globalResolvers, in keyedResolversStore );
        if ( ! ImplementorKey.IsShared )
            return;

        var implementorStore = ReinterpretCast.To<IInternalDependencyKey>( ImplementorKey.Value )
            .GetTargetResolversStore( in globalResolvers, in keyedResolversStore );

        var resolver = GetResolver();
        implementorStore.SharedGenericResolvers.TryAdd(
            new SharedGenericKey( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type ),
            resolver );
    }

    protected sealed override bool TryResolveCreationMethodImmediately(
        UlongSequenceGenerator idGenerator,
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration,
        out DependencyResolver? resolver)
    {
        resolver = null;
        Base.PrepareCreationMethod( idGenerator, availableDependencies, configuration );
        return ! Base.HasState( DependencyResolverFactoryState.Invalid );
    }

    [Pure]
    protected sealed override ConstructorInfo? FindValidConstructor(
        IReadOnlyDictionary<IDependencyKey, DependencyResolverFactory> availableDependencies,
        IDependencyContainerConfigurationBuilder configuration)
    {
        var openCtor = Base.ConstructorInfo;
        Assume.IsNotNull( openCtor );
        var result = openCtor.TryCloseGenericCtor( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type );
        if ( result is null )
            Errors = Errors.Extend(
                Resources.FailedToFindValidCtorForClosedGenericType( Base.ImplementorKey.Value.Type, ImplementorKey.Value.Type ) );

        return result;
    }

    protected sealed override bool ValidateDependencies(
        DependencyLocatorBuilderExtractionParams @params,
        Dictionary<IDependencyKey, DependencyResolverFactory> dynamicResolverFactories,
        IDependencyContainerConfigurationBuilder configuration,
        ref Chain<string> captiveDependencies)
    {
        Base.ValidateRequiredDependencies( @params, dynamicResolverFactories, configuration );
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
                else
                {
                    var baseResolver = ReinterpretCast.To<DependencyResolverFactory>( baseResolution.Value );
                    if ( ! baseResolver.IsOpenGeneric )
                        ParameterResolutions[i] = KeyValuePair.Create( parameter, ( object? )baseResolver );
                    else
                    {
                        var implementorKey = InternalImplementorKey.WithType(
                            baseResolver is OpenGenericRangeDependencyResolverFactory
                                ? parameter.ParameterType
                                : baseResolver.InternalImplementorKey.Type.CloseImplementorType( parameter.ParameterType ) );

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

                        parameterFactory = baseResolver.Close( implementorKey, @params, dynamicResolverFactories );
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
                var baseResolution = member.FindCorrespondingOpenTypeMemberResolution<object>( Base.MemberResolutions );

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
                else
                {
                    var baseResolver = ReinterpretCast.To<DependencyResolverFactory>( baseResolution );
                    if ( ! baseResolver.IsOpenGeneric )
                        MemberResolutions[i] = KeyValuePair.Create( member, ( object? )baseResolver );
                    else
                    {
                        var implementorKey = InternalImplementorKey.WithType(
                            baseResolver is OpenGenericRangeDependencyResolverFactory
                                ? memberType
                                : baseResolver.InternalImplementorKey.Type.CloseImplementorType( memberType ) );

                        if ( @params.ResolverFactories.TryGetValue( implementorKey, out var memberFactory ) )
                        {
                            MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                            captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                            continue;
                        }

                        memberFactory = baseResolver.Close( implementorKey, @params, dynamicResolverFactories );
                        MemberResolutions[i] = KeyValuePair.Create( member, ( object? )memberFactory );
                        captiveDependencies = ValidateCaptiveDependency( captiveDependencies, member, implementorKey, memberFactory );
                    }
                }
            }
        }

        return true;
    }
}
