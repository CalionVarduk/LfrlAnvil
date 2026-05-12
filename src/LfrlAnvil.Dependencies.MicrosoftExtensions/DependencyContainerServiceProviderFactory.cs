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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Extensions;
using LfrlAnvil.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace LfrlAnvil.Dependencies.MicrosoftExtensions;

/// <inheritdoc />
public sealed class DependencyContainerServiceProviderFactory : IServiceProviderFactory<DependencyContainerBuilder>
{
    private readonly Action<DependencyContainerBuilder>? _onBuild;
    private readonly Action<DependencyContainerBuildResult<DependencyContainer>>? _onCreated;
    private readonly bool _supportFromKeyedServicesAttribute;
    private readonly bool _verifyGenericConstraints;

    /// <summary>
    /// Creates a new <see cref="DependencyContainerServiceProviderFactory"/> instance.
    /// </summary>
    /// <param name="supportFromKeyedServicesAttribute">
    /// Specifies whether to support <see cref="FromKeyedServicesAttribute"/> usage in constructor parameters.
    /// Equal to <b>true</b> by default.
    /// </param>
    /// <param name="verifyGenericConstraints">
    /// Specifies whether open generic registrations should verify if their implementor types don't add more generic argument constraints.
    /// Equal to <b>false</b> by default.
    /// </param>
    /// <param name="onBuild">
    /// Optional delegate invoked after created dependency container builder
    /// has been populated with <see cref="IServiceCollection"/> descriptors.
    /// </param>
    /// <param name="onCreated">Optional delegate invoked after dependency container build attempt has been made.</param>
    public DependencyContainerServiceProviderFactory(
        bool supportFromKeyedServicesAttribute = true,
        bool verifyGenericConstraints = false,
        Action<DependencyContainerBuilder>? onBuild = null,
        Action<DependencyContainerBuildResult<DependencyContainer>>? onCreated = null)
    {
        _supportFromKeyedServicesAttribute = supportFromKeyedServicesAttribute;
        _verifyGenericConstraints = verifyGenericConstraints;
        _onBuild = onBuild;
        _onCreated = onCreated;
    }

    /// <inheritdoc />
    [Pure]
    public DependencyContainerBuilder CreateBuilder(IServiceCollection services)
    {
        var result = new DependencyContainerBuilder();
        Populate( result, services );
        _onBuild?.Invoke( result );
        return result;
    }

    /// <inheritdoc />
    [Pure]
    public IServiceProvider CreateServiceProvider(DependencyContainerBuilder containerBuilder)
    {
        containerBuilder.AddSharedImplementor<DependencyContainerServiceProvider>()
            .SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

        AddGlobalServiceProvider<ISupportRequiredService>( containerBuilder );
        AddGlobalServiceProvider<IKeyedServiceProvider>( containerBuilder );
        AddGlobalServiceProvider<IServiceProvider>( containerBuilder );
        AddGlobalServiceProvider<IServiceProviderIsKeyedService>( containerBuilder );
        AddGlobalServiceProvider<IServiceProviderIsService>( containerBuilder );
        AddGlobalServiceProvider<IServiceScope>( containerBuilder );

        containerBuilder.Add<IServiceScopeFactory>()
            .MakeGlobal()
            .SetLifetime( DependencyLifetime.Singleton )
            .FromType<DependencyContainerServiceScopeFactory>()
            .SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );

        if ( _supportFromKeyedServicesAttribute )
        {
            var provider = containerBuilder.Configuration.ConstructorParameterKeyProvider;
            containerBuilder.Configuration.SetConstructorParameterKeyProvider(
                provider is not null
                    ? p => TryReadFromKeyedServicesAttribute( p ) ?? provider( p )
                    : TryReadFromKeyedServicesAttribute );
        }

        containerBuilder.Configuration.EnableOpenGenericArgumentConstraintsVerification( _verifyGenericConstraints );

        var result = containerBuilder.TryBuild();
        _onCreated?.Invoke( result );
        return result.GetContainerOrThrow().RootScope.Locator.Resolve<IServiceProvider>();
    }

    private static void Populate(DependencyContainerBuilder builder, IServiceCollection services)
    {
        var keyedLocatorProviders = new Dictionary<Type, Func<DependencyContainerBuilder, object, IDependencyLocatorBuilder>>();
        foreach ( var descriptor in services )
        {
            var locator = GetLocator( keyedLocatorProviders, builder, descriptor );
            var lifetime = GetLifetime( descriptor );
            var serviceType = descriptor.ServiceType;

            var implementationType = descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType;
            if ( implementationType is not null )
            {
                if ( serviceType.IsGenericTypeDefinition )
                    locator.AddGeneric( serviceType ).SetLifetime( lifetime ).FromType( implementationType );
                else
                    locator.Add( serviceType ).SetLifetime( lifetime ).FromType( implementationType );

                continue;
            }

            if ( descriptor.IsKeyedService )
            {
                if ( descriptor.KeyedImplementationFactory is not null )
                {
                    var key = descriptor.ServiceKey;
                    Assume.IsNotNull( key );

                    var factory = descriptor.KeyedImplementationFactory;
                    locator
                        .Add( serviceType )
                        .SetLifetime( lifetime )
                        .FromFactory( s =>
                        {
                            var serviceProvider = s.Locator.Resolve<IServiceProvider>();
                            return factory( serviceProvider, key );
                        } );

                    continue;
                }
            }
            else if ( descriptor.ImplementationFactory is not null )
            {
                var factory = descriptor.ImplementationFactory;
                locator
                    .Add( serviceType )
                    .SetLifetime( lifetime )
                    .FromFactory( s =>
                    {
                        var serviceProvider = s.Locator.Resolve<IServiceProvider>();
                        return factory( serviceProvider );
                    } );

                continue;
            }

            var instance = descriptor.IsKeyedService ? descriptor.KeyedImplementationInstance : descriptor.ImplementationInstance;
            Assume.IsNotNull( instance );

            locator
                .Add( serviceType )
                .SetLifetime( lifetime )
                .FromFactory( _ => instance )
                .SetDisposalStrategy( DependencyImplementorDisposalStrategy.RenounceOwnership() );
        }
    }

    private static void AddGlobalServiceProvider<T>(DependencyContainerBuilder builder)
        where T : class
    {
        builder.Add<T>().MakeGlobal().SetLifetime( DependencyLifetime.Scoped ).FromSharedImplementor<DependencyContainerServiceProvider>();
    }

    [Pure]
    private static object? TryReadFromKeyedServicesAttribute(ParameterInfo parameter)
    {
        return parameter.GetCustomAttribute<FromKeyedServicesAttribute>()?.Key;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IDependencyLocatorBuilder GetLocator(
        Dictionary<Type, Func<DependencyContainerBuilder, object, IDependencyLocatorBuilder>> keyedLocatorProviders,
        DependencyContainerBuilder builder,
        ServiceDescriptor descriptor)
    {
        if ( ! descriptor.IsKeyedService )
            return builder;

        var key = descriptor.ServiceKey;
        Assume.IsNotNull( key );
        if ( key.Equals( KeyedService.AnyKey ) )
            ExceptionThrower.Throw( new InvalidOperationException( Resources.AnyKeyIsNotSupported ) );

        var keyType = key.GetType();
        ref var provider = ref CollectionsMarshal.GetValueRefOrAddDefault( keyedLocatorProviders, keyType, out var exists )!;
        if ( ! exists )
        {
            var locatorHelperType = typeof( LocatorHelper<> ).MakeGenericType( keyType );
            var getLocatorMethod = locatorHelperType.GetMethod(
                nameof( LocatorHelper<object>.GetLocator ),
                BindingFlags.Static | BindingFlags.NonPublic );

            Assume.IsNotNull( getLocatorMethod );
            provider = getLocatorMethod.CreateDelegate<Func<DependencyContainerBuilder, object, IDependencyLocatorBuilder>>();
        }

        return provider( builder, key );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DependencyLifetime GetLifetime(ServiceDescriptor descriptor)
    {
        return descriptor.Lifetime switch
        {
            ServiceLifetime.Singleton => DependencyLifetime.Singleton,
            ServiceLifetime.Scoped => DependencyLifetime.Scoped,
            _ => DependencyLifetime.Transient
        };
    }

    private sealed class LocatorHelper<TKey>
        where TKey : notnull
    {
        [Pure]
        internal static IDependencyLocatorBuilder GetLocator(DependencyContainerBuilder builder, object key)
        {
            return builder.GetKeyedLocator( ( TKey )key );
        }
    }
}
