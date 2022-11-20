using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Dependencies.Internal.Resolvers;

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
        if ( ! SharedImplementors.TryGetValue( type, out var result ) )
        {
            Ensure.Equals( type.IsGenericTypeDefinition, false, nameof( type ) + '.' + nameof( type.IsGenericTypeDefinition ) );
            result = new DependencyImplementorBuilder( type, DefaultDisposalStrategy );
            SharedImplementors.Add( type, result );
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
    internal virtual IInternalSharedDependencyImplementorKey CreateImplementorKey(Type implementorType)
    {
        return new SharedDependencyImplementorKey( implementorType );
    }

    internal DependencyLocatorBuilderResult Build(DependencyLocatorBuilderParams @params)
    {
        var resolvers = new Dictionary<Type, DependencyResolver>();
        var messages = Chain<DependencyContainerBuildMessages>.Empty;

        foreach ( var (type, builder) in Dependencies )
        {
            if ( builder.InternalSharedImplementorKey is not null )
            {
                var sharedResolver = @params.GetSharedResolver( builder.InternalSharedImplementorKey, builder.Lifetime );
                if ( sharedResolver is null )
                {
                    var implementorBuilder = builder.InternalSharedImplementorKey.GetSharedImplementor( @params.LocatorBuilderStore );
                    if ( implementorBuilder?.Factory is null )
                    {
                        messages = messages.Extend(
                            DependencyContainerBuildMessages.Create(
                                type,
                                builder.InternalSharedImplementorKey,
                                Chain.Create( Resources.ImplementorDoesNotExist( type, builder.InternalSharedImplementorKey ) ),
                                Chain<string>.Empty ) );

                        continue;
                    }

                    sharedResolver = CreateDependencyResolver( @params.GetResolverId(), implementorBuilder, builder.Lifetime );
                    @params.AddSharedResolver( builder.InternalSharedImplementorKey, builder.Lifetime, sharedResolver );
                }

                resolvers.Add( type, sharedResolver );
                continue;
            }

            if ( builder.Implementor?.Factory is null ) // TODO: treat this as Self with auto-discovered ctor in the future
                throw new NotImplementedException();

            var resolver = CreateDependencyResolver( @params.GetResolverId(), builder.Implementor, builder.Lifetime );
            resolvers.Add( type, resolver );
        }

        foreach ( var (type, resolver) in @params.GetDefaultResolvers() )
            resolvers.Add( type, resolver );

        return new DependencyLocatorBuilderResult( resolvers, messages );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DependencyResolver CreateDependencyResolver(ulong id, IDependencyImplementorBuilder builder, DependencyLifetime lifetime)
    {
        Assume.IsNotNull( builder.Factory, nameof( builder.Factory ) );

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
}

internal sealed class DependencyLocatorBuilder<TKey>
    : DependencyLocatorBuilder, IDependencyLocatorBuilder<TKey>, IKeyedDependencyLocatorBuilder
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
    internal override IInternalSharedDependencyImplementorKey CreateImplementorKey(Type implementorType)
    {
        return new SharedDependencyImplementorKey<TKey>( implementorType, Key );
    }

    public Chain<DependencyContainerBuildMessages> BuildKeyed(DependencyLocatorBuilderParams @params)
    {
        var result = Build( @params );
        @params.KeyedResolversStore.AddResolvers( Key, result.Resolvers );
        return result.Messages;
    }
}
