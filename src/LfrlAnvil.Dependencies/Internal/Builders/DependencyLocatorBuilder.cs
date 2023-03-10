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
        Dependencies = new Dictionary<Type, DependencyRangeBuilder>();
    }

    public DependencyLifetime DefaultLifetime { get; private set; }
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy { get; private set; }
    internal Dictionary<Type, DependencyImplementorBuilder> SharedImplementors { get; }
    internal Dictionary<Type, DependencyRangeBuilder> Dependencies { get; }

    Type? IDependencyLocatorBuilder.KeyType => null;
    object? IDependencyLocatorBuilder.Key => null;
    bool IDependencyLocatorBuilder.IsKeyed => false;

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        AssertRegisteredType( type );

        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( SharedImplementors, type, out var exists )!;
        if ( ! exists )
            result = new DependencyImplementorBuilder( this, type );

        return result;
    }

    public IDependencyBuilder Add(Type type)
    {
        var range = GetDependencyRange( type );
        return range.Add();
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

    public IDependencyRangeBuilder GetDependencyRange(Type type)
    {
        AssertRegisteredType( type );

        ref var range = ref CollectionsMarshal.GetValueRefOrAddDefault( Dependencies, type, out var exists )!;
        if ( ! exists )
            range = new DependencyRangeBuilder( this, type );

        return range;
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

        foreach ( var rangeBuilder in Dependencies.Values )
        {
            var dependencyKey = CreateImplementorKey( rangeBuilder.DependencyType );
            var builderSpan = CollectionsMarshal.AsSpan( rangeBuilder.InternalElements );
            var builder = builderSpan.Length > 0 ? builderSpan[^1] : null;
            DependencyResolverFactory? builderFactory = null;

            if ( builder is not null )
            {
                builderFactory = ExtractResolverFactory(
                    locatorBuilderStore,
                    @params,
                    ImplementorKey.Create( dependencyKey ),
                    builder,
                    out var m );

                messages = messages.Extend( m );
                @params.ResolverFactories.Add( dependencyKey, builderFactory );
            }

            var rangeDependencyType = typeof( IEnumerable<> ).MakeGenericType( rangeBuilder.DependencyType );
            if ( Dependencies.ContainsKey( rangeDependencyType ) )
                continue;

            var rangeBuilderCount = 0;
            foreach ( var b in builderSpan )
            {
                if ( b.IsIncludedInRange )
                    ++rangeBuilderCount;
            }

            var factoriesInRange = rangeBuilderCount > 0 ? new DependencyResolverFactory[rangeBuilderCount] : null;
            var factoriesInRangeToCreate = rangeBuilderCount;
            if ( builder is not null && builder.IsIncludedInRange )
            {
                Assume.IsNotNull( factoriesInRange, nameof( factoriesInRange ) );
                Assume.IsNotNull( builderFactory, nameof( builderFactory ) );
                factoriesInRange[^1] = builderFactory;
                --factoriesInRangeToCreate;
            }

            var builderIndex = 0;
            for ( var i = 0; i < factoriesInRangeToCreate; ++i )
            {
                Assume.IsNotNull( factoriesInRange, nameof( factoriesInRange ) );
                while ( ! builderSpan[builderIndex].IsIncludedInRange )
                    ++builderIndex;

                builder = builderSpan[builderIndex++];
                builderFactory = ExtractResolverFactory(
                    locatorBuilderStore,
                    @params,
                    ImplementorKey.Create( dependencyKey, i ),
                    builder,
                    out var m );

                messages = messages.Extend( m );
                factoriesInRange[i] = builderFactory;
            }

            var rangeDependencyKey = ImplementorKey.Create( CreateImplementorKey( rangeDependencyType ) );
            var rangeFactory = new RangeDependencyResolverFactory( rangeDependencyKey, rangeBuilder.OnResolvingCallback, factoriesInRange );
            @params.ResolverFactories.Add( rangeDependencyKey.Value, rangeFactory );
        }

        @params.AddDefaultResolverFactories( this );
        return messages;
    }

    private DependencyResolverFactory ExtractResolverFactory(
        DependencyLocatorBuilderStore locatorBuilderStore,
        DependencyLocatorBuilderExtractionParams @params,
        ImplementorKey key,
        DependencyBuilder builder,
        out Chain<DependencyContainerBuildMessages> messages)
    {
        var errors = Chain<string>.Empty;

        if ( builder.InternalSharedImplementorKey is null )
        {
            messages = Chain<DependencyContainerBuildMessages>.Empty;
            var implementorBuilder = builder.Implementor ?? new DependencyImplementorBuilder( this, builder.DependencyType );
            return DependencyResolverFactory.Create( key, builder.Lifetime, implementorBuilder );
        }

        if ( ! builder.InternalSharedImplementorKey.Type.IsAssignableTo( key.Value.Type ) )
            errors = errors.Extend( Resources.ProvidedSharedImplementorTypeIsIncorrect( builder.InternalSharedImplementorKey.Type ) );

        var sharedFactory = @params.GetSharedResolverFactory( builder.InternalSharedImplementorKey, builder.Lifetime );
        if ( sharedFactory is null )
        {
            var sharedImplementorBuilder = builder.InternalSharedImplementorKey.GetSharedImplementor( locatorBuilderStore );
            if ( sharedImplementorBuilder is null )
            {
                errors = errors.Extend( Resources.SharedImplementorIsMissing( builder.InternalSharedImplementorKey ) );
                messages = Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) );
                return DependencyResolverFactory.CreateInvalid( key, builder.Lifetime );
            }

            sharedFactory = DependencyResolverFactory.Create(
                ImplementorKey.CreateShared( builder.InternalSharedImplementorKey ),
                builder.Lifetime,
                sharedImplementorBuilder );

            @params.AddSharedResolverFactory( builder.InternalSharedImplementorKey, builder.Lifetime, sharedFactory );
        }

        messages = errors.Count > 0
            ? Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) )
            : Chain<DependencyContainerBuildMessages>.Empty;

        return sharedFactory;
    }

    private static void AssertRegisteredType(Type type)
    {
        if ( type.IsGenericTypeDefinition || type.ContainsGenericParameters )
            throw new InvalidTypeRegistrationException( type, nameof( type ) );
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
