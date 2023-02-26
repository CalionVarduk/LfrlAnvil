using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal class DependencyLocatorBuilder : IDependencyLocatorBuilder
{
    internal DependencyLocatorBuilder()
    {
        DefaultLifetime = DependencyLifetime.Transient;
        DefaultDisposalStrategy = DependencyImplementorDisposalStrategy.UseDisposableInterface();
        SharedImplementors = new Dictionary<Type, DependencyImplementorBuilder>();
        Dependencies = new Dictionary<Type, DependencyBuilder>();
    }

    public DependencyLifetime DefaultLifetime { get; private set; }
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy { get; private set; }
    internal Dictionary<Type, DependencyImplementorBuilder> SharedImplementors { get; }
    internal Dictionary<Type, DependencyBuilder> Dependencies { get; }

    Type? IDependencyLocatorBuilder.KeyType => null;
    object? IDependencyLocatorBuilder.Key => null;
    bool IDependencyLocatorBuilder.IsKeyed => false;

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( SharedImplementors, type, out var exists )!;
        if ( ! exists )
        {
            Ensure.Equals( type.IsGenericTypeDefinition, false, nameof( type ) + '.' + nameof( type.IsGenericTypeDefinition ) );
            result = new DependencyImplementorBuilder( this, type );
        }

        return result;
    }

    public IDependencyBuilder Add(Type type)
    {
        Ensure.Equals( type.IsGenericTypeDefinition, false, nameof( type ) + '.' + nameof( type.IsGenericTypeDefinition ) );
        var dependency = new DependencyBuilder( this, type, DefaultLifetime );
        Dependencies[type] = dependency;
        return dependency;
    }

    public IDependencyLocatorBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime, nameof( lifetime ) );
        DefaultLifetime = lifetime;
        return this;
    }

    public IDependencyLocatorBuilder SetDefaultDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        DefaultDisposalStrategy = strategy;
        return this;
    }

    [Pure]
    public IDependencyImplementorBuilder? TryGetSharedImplementor(Type type)
    {
        return SharedImplementors.GetValueOrDefault( type );
    }

    [Pure]
    public IDependencyBuilder? TryGetDependency(Type type)
    {
        return Dependencies.GetValueOrDefault( type );
    }

    [Pure]
    internal virtual IInternalDependencyKey CreateImplementorKey(Type implementorType)
    {
        return new DependencyKey( implementorType );
    }

    internal Chain<DependencyContainerBuildMessages> ExtractResolverFactories(
        DependencyLocatorBuilderStore locatorBuilderStore,
        DependencyLocatorBuilderExtractionParams @params)
    {
        var messages = Chain<DependencyContainerBuildMessages>.Empty;

        foreach ( var builder in Dependencies.Values )
        {
            var dependencyKey = ImplementorKey.Create( CreateImplementorKey( builder.DependencyType ) );

            if ( builder.InternalSharedImplementorKey is not null )
            {
                var sharedFactory = @params.GetSharedResolverFactory( builder.InternalSharedImplementorKey, builder.Lifetime );
                if ( sharedFactory is null )
                {
                    var implementorBuilder = builder.InternalSharedImplementorKey.GetSharedImplementor( locatorBuilderStore );
                    if ( implementorBuilder is null )
                    {
                        var errorMessage = Resources.SharedImplementorIsMissing( builder.InternalSharedImplementorKey );
                        messages = messages.Extend(
                            DependencyContainerBuildMessages.CreateErrors( dependencyKey, Chain.Create( errorMessage ) ) );

                        continue;
                    }

                    sharedFactory = DependencyResolverFactory.Create(
                        ImplementorKey.CreateShared( builder.InternalSharedImplementorKey ),
                        implementorBuilder,
                        builder.Lifetime );

                    @params.AddSharedResolverFactory( builder.InternalSharedImplementorKey, builder.Lifetime, sharedFactory );
                }

                @params.ResolverFactories.Add( dependencyKey.Value, sharedFactory );
                continue;
            }

            var factory = DependencyResolverFactory.Create(
                dependencyKey,
                builder.Implementor ?? new DependencyImplementorBuilder( this, builder.DependencyType ),
                builder.Lifetime );

            @params.ResolverFactories.Add( dependencyKey.Value, factory );
        }

        @params.AddDefaultResolverFactories( this );
        return messages;
    }
}

internal sealed class DependencyLocatorBuilder<TKey> : DependencyLocatorBuilder, IDependencyLocatorBuilder<TKey>
    where TKey : notnull
{
    internal DependencyLocatorBuilder(TKey key)
    {
        Key = key;
    }

    public TKey Key { get; }

    Type IDependencyLocatorBuilder.KeyType => typeof( TKey );
    object IDependencyLocatorBuilder.Key => Key;
    bool IDependencyLocatorBuilder.IsKeyed => true;

    [Pure]
    internal override IInternalDependencyKey CreateImplementorKey(Type implementorType)
    {
        return new DependencyKey<TKey>( implementorType, Key );
    }
}
