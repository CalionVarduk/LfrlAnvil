using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies;

public class DependencyContainerBuilder : IDependencyContainerBuilder
{
    private readonly DependencyLocatorBuilder _locatorBuilder;

    public DependencyContainerBuilder()
    {
        _locatorBuilder = new DependencyLocatorBuilder();
    }

    public DependencyLifetime DefaultLifetime => _locatorBuilder.DefaultLifetime;

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
            var resolverId = resolverIdGenerator.Generate();

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

                    sharedResolver = builder.Lifetime switch
                    {
                        DependencyLifetime.Singleton => new SingletonDependencyResolver(
                            resolverId,
                            builder.SharedImplementorType,
                            implementorBuilder.Factory ),
                        DependencyLifetime.ScopedSingleton => new ScopedSingletonDependencyResolver(
                            resolverId,
                            builder.SharedImplementorType,
                            implementorBuilder.Factory ),
                        DependencyLifetime.Scoped => new ScopedDependencyResolver(
                            resolverId,
                            builder.SharedImplementorType,
                            implementorBuilder.Factory ),
                        _ => new TransientDependencyResolver( resolverId, builder.SharedImplementorType, implementorBuilder.Factory )
                    };

                    sharedImplementors.Add( (builder.SharedImplementorType, builder.Lifetime), sharedResolver );
                }

                resolvers.Add( type, sharedResolver );
                continue;
            }

            if ( builder.Implementor?.Factory is null ) // TODO: treat this as Self with auto-discovered ctor in the future
                throw new NotImplementedException();

            DependencyResolver resolver = builder.Lifetime switch
            {
                DependencyLifetime.Singleton => new SingletonDependencyResolver( resolverId, type, builder.Implementor.Factory ),
                DependencyLifetime.ScopedSingleton => new ScopedSingletonDependencyResolver(
                    resolverId,
                    type,
                    builder.Implementor.Factory ),
                DependencyLifetime.Scoped => new ScopedDependencyResolver( resolverId, type, builder.Implementor.Factory ),
                _ => new TransientDependencyResolver( resolverId, type, builder.Implementor.Factory )
            };

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
}
