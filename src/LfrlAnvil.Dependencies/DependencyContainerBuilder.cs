﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Extensions;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

public class DependencyContainerBuilder : IDependencyContainerBuilder
{
    private readonly DependencyLocatorBuilder _locatorBuilder;

    public DependencyContainerBuilder()
    {
        _locatorBuilder = new DependencyLocatorBuilder();
        InjectablePropertyType = typeof( Injected<> );
        OptionalDependencyAttributeType = typeof( AllowNullAttribute );
    }

    public Type InjectablePropertyType { get; private set; }
    public Type OptionalDependencyAttributeType { get; private set; }
    public DependencyLifetime DefaultLifetime => _locatorBuilder.DefaultLifetime;
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy => _locatorBuilder.DefaultDisposalStrategy;

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        return _locatorBuilder.AddSharedImplementor( type );
    }

    public IDependencyBuilder Add(Type type)
    {
        return _locatorBuilder.Add( type );
    }

    public DependencyContainerBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        _locatorBuilder.SetDefaultLifetime( lifetime );
        return this;
    }

    public DependencyContainerBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        _locatorBuilder.SetDefaultDisposalStrategy( strategy );
        return this;
    }

    public DependencyContainerBuilder SetInjectablePropertyType(Type openGenericType)
    {
        if ( ! IsInjectablePropertyTypeCorrect( openGenericType ) )
        {
            throw new DependencyContainerBuilderConfigurationException(
                Resources.InvalidInjectablePropertyType( openGenericType ),
                nameof( openGenericType ) );
        }

        InjectablePropertyType = openGenericType;
        return this;
    }

    public DependencyContainerBuilder SetOptionalDependencyAttributeType(Type attributeType)
    {
        if ( ! IsOptionalDependencyAttributeTypeCorrect( attributeType ) )
        {
            throw new DependencyContainerBuilderConfigurationException(
                Resources.InvalidOptionalDependencyAttributeType( attributeType ),
                nameof( attributeType ) );
        }

        OptionalDependencyAttributeType = attributeType;
        return this;
    }

    [Pure]
    public IDependencyImplementorBuilder? TryGetSharedImplementor(Type type)
    {
        return _locatorBuilder.TryGetSharedImplementor( type );
    }

    [Pure]
    public IDependencyBuilder? TryGetDependency(Type type)
    {
        return _locatorBuilder.TryGetDependency( type );
    }

    [Pure]
    public DependencyContainerBuildResult<DependencyContainer> TryBuild()
    {
        var sharedImplementors = new Dictionary<(Type, DependencyLifetime), DependencyResolver>();
        var resolvers = new Dictionary<Type, DependencyResolver>();
        var resolverIdGenerator = new UlongSequenceGenerator();

        var typeMessages = Chain<DependencyContainerBuildMessages>.Empty;
        var hasErrors = false;

        foreach ( var (type, builder) in _locatorBuilder.Dependencies )
        {
            if ( builder.SharedImplementorType is not null )
            {
                if ( ! sharedImplementors.TryGetValue( (builder.SharedImplementorType, builder.Lifetime), out var sharedResolver ) )
                {
                    if ( ! _locatorBuilder.SharedImplementors.TryGetValue( builder.SharedImplementorType, out var implementorBuilder ) ||
                        implementorBuilder.Factory is null )
                    {
                        typeMessages = typeMessages.Extend(
                            DependencyContainerBuildMessages.Create(
                                type,
                                builder.SharedImplementorType,
                                Chain.Create( Resources.ImplementorDoesNotExist( type, builder.SharedImplementorType ) ),
                                Chain<string>.Empty ) );

                        hasErrors = true;
                        continue;
                    }

                    sharedResolver = CreateDependencyResolver( resolverIdGenerator, implementorBuilder, builder.Lifetime );
                    sharedImplementors.Add( (builder.SharedImplementorType, builder.Lifetime), sharedResolver );
                }

                resolvers.Add( type, sharedResolver );
                continue;
            }

            if ( builder.Implementor?.Factory is null ) // TODO: treat this as Self with auto-discovered ctor in the future
                throw new NotImplementedException();

            var resolver = CreateDependencyResolver( resolverIdGenerator, builder.Implementor, builder.Lifetime );
            resolvers.Add( type, resolver );
        }

        if ( hasErrors )
            return new DependencyContainerBuildResult<DependencyContainer>( null, typeMessages );

        resolvers.Add( typeof( IDependencyContainer ), new DependencyContainerResolver( resolverIdGenerator.Generate() ) );
        resolvers.Add( typeof( IDependencyScope ), new DependencyScopeResolver( resolverIdGenerator.Generate() ) );
        resolvers.Add( typeof( IDependencyLocator ), new DependencyLocatorResolver( resolverIdGenerator.Generate() ) );

        var result = new DependencyContainer( resolvers );
        return new DependencyContainerBuildResult<DependencyContainer>( result, typeMessages );
    }

    [Pure]
    DependencyContainerBuildResult<IDisposableDependencyContainer> IDependencyContainerBuilder.TryBuild()
    {
        var result = TryBuild();
        return new DependencyContainerBuildResult<IDisposableDependencyContainer>( result.Container, result.Messages );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetDefaultLifetime(DependencyLifetime lifetime)
    {
        return SetDefaultLifetime( lifetime );
    }

    IDependencyLocatorBuilder IDependencyLocatorBuilder.SetDefaultLifetime(DependencyLifetime lifetime)
    {
        return ReinterpretCast.To<IDependencyContainerBuilder>( this ).SetDefaultLifetime( lifetime );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        return SetDefaultDisposalStrategy( strategy );
    }

    IDependencyLocatorBuilder IDependencyLocatorBuilder.SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        return ReinterpretCast.To<IDependencyContainerBuilder>( this ).SetDefaultDisposalStrategy( strategy );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetInjectablePropertyType(Type openGenericType)
    {
        return SetInjectablePropertyType( openGenericType );
    }

    IDependencyContainerBuilder IDependencyContainerBuilder.SetOptionalDependencyAttributeType(Type attributeType)
    {
        return SetOptionalDependencyAttributeType( attributeType );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DependencyResolver CreateDependencyResolver(
        UlongSequenceGenerator idGenerator,
        IDependencyImplementorBuilder builder,
        DependencyLifetime lifetime)
    {
        Assume.IsNotNull( builder.Factory, nameof( builder.Factory ) );

        var id = idGenerator.Generate();

        DependencyResolver result = lifetime switch
        {
            DependencyLifetime.Singleton => new SingletonDependencyResolver(
                id,
                builder.ImplementorType,
                builder.DisposalStrategy,
                builder.OnResolvingCallback,
                builder.Factory ),
            DependencyLifetime.ScopedSingleton => new ScopedSingletonDependencyResolver(
                id,
                builder.ImplementorType,
                builder.DisposalStrategy,
                builder.OnResolvingCallback,
                builder.Factory ),
            DependencyLifetime.Scoped => new ScopedDependencyResolver(
                id,
                builder.ImplementorType,
                builder.DisposalStrategy,
                builder.OnResolvingCallback,
                builder.Factory ),
            _ => new TransientDependencyResolver(
                id,
                builder.ImplementorType,
                builder.DisposalStrategy,
                builder.OnResolvingCallback,
                builder.Factory )
        };

        return result;
    }

    [Pure]
    private static bool IsInjectablePropertyTypeCorrect(Type type)
    {
        if ( ! type.IsGenericTypeDefinition )
            return false;

        var genericArgs = type.GetGenericArguments();
        if ( genericArgs.Length != 1 )
            return false;

        var instanceType = genericArgs[0];
        var ctor = type.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
            .FirstOrDefault(
                c =>
                {
                    var parameters = c.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == instanceType;
                } );

        return ctor is not null;
    }

    [Pure]
    private static bool IsOptionalDependencyAttributeTypeCorrect(Type type)
    {
        if ( type.IsGenericTypeDefinition )
            return false;

        if ( type.Visit( t => t.BaseType ).All( t => t != typeof( Attribute ) ) )
            return false;

        var attributeUsage = type.Visit( t => t.BaseType )
            .Prepend( type )
            .Select( t => t.GetAttribute<AttributeUsageAttribute>( inherit: false ) )
            .FirstOrDefault( a => a is not null );

        return attributeUsage is not null && (attributeUsage.ValidOn & AttributeTargets.Parameter) == AttributeTargets.Parameter;
    }
}
