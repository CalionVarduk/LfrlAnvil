// Copyright 2024-2026 Łukasz Furlepa
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
using System.Runtime.CompilerServices;
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
        SharedImplementors = new Dictionary<Type, object>();
        Dependencies = new Dictionary<Type, IInternalDependencyRangeBuilder>();
    }

    public DependencyLifetime DefaultLifetime { get; private set; }
    public DependencyImplementorDisposalStrategy DefaultDisposalStrategy { get; private set; }
    internal Dictionary<Type, object> SharedImplementors { get; }
    internal Dictionary<Type, IInternalDependencyRangeBuilder> Dependencies { get; }

    Type? IDependencyLocatorBuilder.KeyType => null;
    object? IDependencyLocatorBuilder.Key => null;
    bool IDependencyLocatorBuilder.IsKeyed => false;

    public IDependencyImplementorBuilder AddSharedImplementor(Type type)
    {
        AssertRegisteredType( type );

        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( SharedImplementors, type, out var exists )!;
        if ( ! exists )
            result = new DependencyImplementorBuilder( this, type );

        return ReinterpretCast.To<DependencyImplementorBuilder>( result );
    }

    public IOpenGenericDependencyImplementorBuilder AddSharedGenericImplementor(Type type)
    {
        AssertRegisteredGenericType( type );

        ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( SharedImplementors, type, out var exists )!;
        if ( ! exists )
            result = new OpenGenericDependencyImplementorBuilder( this, type );

        return ReinterpretCast.To<OpenGenericDependencyImplementorBuilder>( result );
    }

    public IDependencyBuilder Add(Type type)
    {
        var range = GetDependencyRange( type );
        return range.Add();
    }

    public IOpenGenericDependencyBuilder AddGeneric(Type type)
    {
        var range = GetGenericDependencyRange( type );
        return range.Add();
    }

    public IDependencyLocatorBuilder SetDefaultLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime );
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
        return SharedImplementors.GetValueOrDefault( type ) as IDependencyImplementorBuilder;
    }

    [Pure]
    public IOpenGenericDependencyImplementorBuilder? TryGetSharedGenericImplementor(Type type)
    {
        return SharedImplementors.GetValueOrDefault( type ) as IOpenGenericDependencyImplementorBuilder;
    }

    public IDependencyRangeBuilder GetDependencyRange(Type type)
    {
        AssertRegisteredType( type );

        OpenGenericDependencyRangeBuilder? openGenericBuilder = null;
        if ( type.IsGenericType )
        {
            var openType = type.GetGenericTypeDefinition();
            if ( openType != typeof( IEnumerable<> ) )
                openGenericBuilder = ReinterpretCast.To<OpenGenericDependencyRangeBuilder>( GetGenericDependencyRange( openType ) );
        }

        ref var range = ref CollectionsMarshal.GetValueRefOrAddDefault( Dependencies, type, out var exists )!;
        if ( ! exists )
            range = new DependencyRangeBuilder( this, type, openGenericBuilder );

        return ReinterpretCast.To<DependencyRangeBuilder>( range );
    }

    public IOpenGenericDependencyRangeBuilder GetGenericDependencyRange(Type type)
    {
        AssertRegisteredGenericType( type );

        ref var range = ref CollectionsMarshal.GetValueRefOrAddDefault( Dependencies, type, out var exists )!;
        if ( ! exists )
            range = new OpenGenericDependencyRangeBuilder( this, type );

        return ReinterpretCast.To<OpenGenericDependencyRangeBuilder>( range );
    }

    [Pure]
    internal virtual IInternalDependencyKey CreateImplementorKey(Type implementorType)
    {
        return new DependencyKey( implementorType );
    }

    internal Chain<DependencyContainerBuildMessages> ExtractResolverFactories(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params)
    {
        var messages = Chain<DependencyContainerBuildMessages>.Empty;

        foreach ( var rangeBuilder in Dependencies.Values )
        {
            if ( rangeBuilder.IsOpenGeneric )
                ExtractOpenTypeResolverFactories(
                    locatorBuilderStore,
                    in @params,
                    ReinterpretCast.To<OpenGenericDependencyRangeBuilder>( rangeBuilder ),
                    ref messages );
            else
                ExtractClosedTypeResolverFactories(
                    locatorBuilderStore,
                    in @params,
                    ReinterpretCast.To<DependencyRangeBuilder>( rangeBuilder ),
                    ref messages );
        }

        return messages;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ExtractClosedTypeResolverFactories(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params,
        DependencyRangeBuilder rangeBuilder,
        ref Chain<DependencyContainerBuildMessages> messages)
    {
        var dependencyKey = CreateImplementorKey( rangeBuilder.DependencyType );
        var builderSpan = CollectionsMarshal.AsSpan( rangeBuilder.InternalElements );
        var builder = builderSpan.Length > 0 ? builderSpan[^1] : null;
        DependencyResolverFactory? builderFactory = null;

        if ( builder is not null )
        {
            builderFactory = ExtractAnyResolverFactory(
                locatorBuilderStore,
                in @params,
                ImplementorKey.Create( dependencyKey ),
                builder,
                isLastInRange: true,
                out var m );

            messages = messages.Extend( m );
            if ( ! builder.IsOpenGeneric )
                @params.ResolverFactories.Add( dependencyKey, builderFactory );

            if ( builder.IsGlobal )
                @params.RegisterCustomDefaultResolverFactory( rangeBuilder.DependencyType, builderFactory );
        }

        var rangeDependencyType = typeof( IEnumerable<> ).MakeGenericType( rangeBuilder.DependencyType );
        if ( Dependencies.ContainsKey( rangeDependencyType ) )
            return;

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
            Assume.IsNotNull( factoriesInRange );
            Assume.IsNotNull( builderFactory );
            factoriesInRange[^1] = builderFactory;
            --factoriesInRangeToCreate;
        }

        var builderIndex = 0;
        for ( var i = 0; i < factoriesInRangeToCreate; ++i )
        {
            Assume.IsNotNull( factoriesInRange );
            while ( ! builderSpan[builderIndex].IsIncludedInRange )
                ++builderIndex;

            builder = builderSpan[builderIndex++];
            builderFactory = ExtractAnyResolverFactory(
                locatorBuilderStore,
                in @params,
                ImplementorKey.Create( dependencyKey, i ),
                builder,
                isLastInRange: false,
                out var m );

            messages = messages.Extend( m );
            factoriesInRange[i] = builderFactory;
        }

        var rangeDependencyKey = ImplementorKey.Create( CreateImplementorKey( rangeDependencyType ) );
        var rangeFactory = new ClosedRangeDependencyResolverFactory(
            rangeDependencyKey,
            rangeBuilder.OnResolvingCallback,
            factoriesInRange );

        @params.ResolverFactories.Add( rangeDependencyKey.Value, rangeFactory );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ExtractOpenTypeResolverFactories(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params,
        OpenGenericDependencyRangeBuilder rangeBuilder,
        ref Chain<DependencyContainerBuildMessages> messages)
    {
        var builderSpan = CollectionsMarshal.AsSpan( rangeBuilder.InternalElements );
        if ( builderSpan.Length == 0 )
            return;

        var dependencyKey = CreateImplementorKey( rangeBuilder.DependencyType );
        var builder = builderSpan[^1];
        var builderFactory = ExtractGenericResolverFactory(
            locatorBuilderStore,
            in @params,
            ImplementorKey.Create( dependencyKey ),
            builder,
            isLastInRange: true,
            out var m );

        messages = messages.Extend( m );
        @params.ResolverFactories.Add( dependencyKey, builderFactory );

        var rangeDependencyType = typeof( IEnumerable<> ).MakeGenericType( rangeBuilder.DependencyType );
        if ( Dependencies.ContainsKey( rangeDependencyType ) )
            return;

        var rangeBuilderCount = 0;
        foreach ( var b in builderSpan )
        {
            if ( b.IsIncludedInRange )
                ++rangeBuilderCount;
        }

        var factoriesInRange = rangeBuilderCount > 0 ? new DependencyResolverFactory[rangeBuilderCount] : null;
        var factoriesInRangeToCreate = rangeBuilderCount;
        if ( builder.IsIncludedInRange )
        {
            Assume.IsNotNull( factoriesInRange );
            Assume.IsNotNull( builderFactory );
            factoriesInRange[^1] = builderFactory;
            --factoriesInRangeToCreate;
        }

        var builderIndex = 0;
        for ( var i = 0; i < factoriesInRangeToCreate; ++i )
        {
            Assume.IsNotNull( factoriesInRange );
            while ( ! builderSpan[builderIndex].IsIncludedInRange )
                ++builderIndex;

            builder = builderSpan[builderIndex++];
            builderFactory = ExtractGenericResolverFactory(
                locatorBuilderStore,
                in @params,
                ImplementorKey.Create( dependencyKey, i ),
                builder,
                isLastInRange: false,
                out m );

            messages = messages.Extend( m );
            factoriesInRange[i] = builderFactory;
        }

        var rangeDependencyKey = ImplementorKey.Create( CreateImplementorKey( rangeDependencyType ) );
        var rangeFactory = new OpenGenericRangeDependencyResolverFactory(
            rangeDependencyKey,
            rangeBuilder.OnResolvingCallback,
            factoriesInRange );

        @params.ResolverFactories.Add( rangeDependencyKey.Value, rangeFactory );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DependencyResolverFactory ExtractAnyResolverFactory(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params,
        ImplementorKey closedKey,
        IInternalDependencyBuilder builder,
        bool isLastInRange,
        out Chain<DependencyContainerBuildMessages> messages)
    {
        if ( ! builder.IsOpenGeneric )
            return ExtractResolverFactory(
                locatorBuilderStore,
                in @params,
                closedKey,
                ReinterpretCast.To<DependencyBuilder>( builder ),
                isLastInRange,
                out messages );

        var openBuilder = ReinterpretCast.To<OpenGenericDependencyBuilder>( builder );
        var openDependencyKey = ReinterpretCast.To<IInternalDependencyKey>( closedKey.Value ).WithType( openBuilder.DependencyType );
        var openResolverFactory = ExtractGenericResolverFactory(
            locatorBuilderStore,
            in @params,
            closedKey.IsShared
                ? ImplementorKey.CreateShared( openDependencyKey )
                : ImplementorKey.Create( openDependencyKey, closedKey.RangeIndex ),
            openBuilder,
            isLastInRange,
            out messages );

        return openResolverFactory.Close( ReinterpretCast.To<IInternalDependencyKey>( closedKey.Value ), in @params );
    }

    private DependencyResolverFactory ExtractResolverFactory(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params,
        ImplementorKey key,
        DependencyBuilder builder,
        bool isLastInRange,
        out Chain<DependencyContainerBuildMessages> messages)
    {
        var errors = Chain<string>.Empty;

        if ( builder.InternalSharedImplementorKey is null )
        {
            messages = Chain<DependencyContainerBuildMessages>.Empty;
            var implementorBuilder = builder.Implementor ?? new DependencyImplementorBuilder( this, builder.DependencyType );
            return DependencyResolverFactory.Create(
                key,
                builder.Lifetime,
                ReinterpretCast.To<DependencyImplementorBuilder>( implementorBuilder ) );
        }

        if ( ! builder.InternalSharedImplementorKey.Type.IsAssignableTo( key.Value.Type ) )
            errors = errors.Extend( Resources.ProvidedSharedImplementorTypeIsIncorrect( builder.InternalSharedImplementorKey.Type ) );

        var sharedFactory = @params.GetSharedResolverFactory( builder.InternalSharedImplementorKey, builder.Lifetime );
        if ( sharedFactory is null )
        {
            var sharedImplementorBuilder = builder.InternalSharedImplementorKey.GetSharedImplementor( locatorBuilderStore );
            if ( sharedImplementorBuilder is null )
            {
                if ( ! builder.InternalSharedImplementorKey.Type.IsGenericType )
                {
                    errors = errors.Extend( Resources.SharedImplementorIsMissing( builder.InternalSharedImplementorKey ) );
                    messages = Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) );
                    return DependencyResolverFactory.CreateInvalid( key, builder.Lifetime );
                }

                var openGenericKey = builder.InternalSharedImplementorKey.WithType(
                    builder.InternalSharedImplementorKey.Type.GetGenericTypeDefinition() );

                var sharedGenericFactory = TryGetSharedGenericResolverFactory(
                    locatorBuilderStore,
                    in @params,
                    openGenericKey,
                    builder.Lifetime,
                    isLastInRange );

                if ( sharedGenericFactory is null )
                {
                    errors = errors.Extend( Resources.SharedImplementorIsMissing( builder.InternalSharedImplementorKey ) );
                    messages = Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) );
                    return DependencyResolverFactory.CreateInvalid( key, builder.Lifetime );
                }

                sharedFactory = sharedGenericFactory.CloseShared( builder.InternalSharedImplementorKey, in @params );
            }
            else
            {
                sharedFactory = DependencyResolverFactory.Create(
                    ImplementorKey.CreateShared( builder.InternalSharedImplementorKey ),
                    builder.Lifetime,
                    sharedImplementorBuilder );

                @params.AddSharedResolverFactory( builder.InternalSharedImplementorKey, builder.Lifetime, sharedFactory );
            }
        }

        messages = errors.Count > 0
            ? Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) )
            : Chain<DependencyContainerBuildMessages>.Empty;

        return sharedFactory;
    }

    private DependencyResolverFactory ExtractGenericResolverFactory(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params,
        ImplementorKey key,
        OpenGenericDependencyBuilder builder,
        bool isLastInRange,
        out Chain<DependencyContainerBuildMessages> messages)
    {
        var errors = Chain<string>.Empty;

        if ( builder.InternalSharedImplementorKey is null )
        {
            messages = Chain<DependencyContainerBuildMessages>.Empty;
            var implementorBuilder = builder.Implementor ?? new OpenGenericDependencyImplementorBuilder( this, builder.DependencyType );
            return new OpenGenericDependencyResolverFactory( key, builder.Lifetime, implementorBuilder, isLastInRange );
        }

        if ( ! builder.InternalSharedImplementorKey.Type.IsOpenGenericAssignableTo( key.Value.Type ) )
            errors = errors.Extend( Resources.ProvidedSharedImplementorTypeIsIncorrect( builder.InternalSharedImplementorKey.Type ) );

        var openDependencyKey = builder.InternalSharedImplementorKey;
        if ( ! builder.InternalSharedImplementorKey.Type.IsGenericTypeDefinition
            && builder.InternalSharedImplementorKey.Type.IsGenericType )
            openDependencyKey = builder.InternalSharedImplementorKey.WithType(
                builder.InternalSharedImplementorKey.Type.GetGenericTypeDefinition() );

        var sharedFactory = TryGetSharedGenericResolverFactory(
            locatorBuilderStore,
            in @params,
            openDependencyKey,
            builder.Lifetime,
            isLastInRange );

        if ( sharedFactory is null )
        {
            errors = errors.Extend( Resources.SharedImplementorIsMissing( builder.InternalSharedImplementorKey ) );
            messages = Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) );
            return DependencyResolverFactory.CreateInvalid( key, builder.Lifetime, isOpenGeneric: true );
        }

        messages = errors.Count > 0
            ? Chain.Create( DependencyContainerBuildMessages.CreateErrors( key, errors ) )
            : Chain<DependencyContainerBuildMessages>.Empty;

        return ReferenceEquals( openDependencyKey, builder.InternalSharedImplementorKey )
            ? sharedFactory
            : new PartiallyOpenGenericSharedDependencyResolverFactory( builder.InternalSharedImplementorKey, sharedFactory );
    }

    private static OpenGenericDependencyResolverFactory? TryGetSharedGenericResolverFactory(
        DependencyLocatorBuilderStore locatorBuilderStore,
        in DependencyLocatorBuilderExtractionParams @params,
        IInternalDependencyKey dependencyKey,
        DependencyLifetime lifetime,
        bool isLastInRange)
    {
        if ( ! dependencyKey.Type.IsGenericTypeDefinition )
            return null;

        var factory = @params.GetSharedResolverFactory( dependencyKey, lifetime );
        if ( factory is not null )
            return ReinterpretCast.To<OpenGenericDependencyResolverFactory>( factory );

        var sharedImplementorBuilder = dependencyKey.GetSharedGenericImplementor( locatorBuilderStore );
        if ( sharedImplementorBuilder is null )
            return null;

        factory = new OpenGenericDependencyResolverFactory(
            ImplementorKey.CreateShared( dependencyKey ),
            lifetime,
            sharedImplementorBuilder,
            isLastInRange );

        @params.AddSharedResolverFactory( dependencyKey, lifetime, factory );
        return ReinterpretCast.To<OpenGenericDependencyResolverFactory>( factory );
    }

    private static void AssertRegisteredType(Type type)
    {
        if ( type.ContainsGenericParameters )
            throw new InvalidTypeRegistrationException( type, nameof( type ) );
    }

    private static void AssertRegisteredGenericType(Type type)
    {
        if ( ! type.IsGenericTypeDefinition || type == typeof( IEnumerable<> ) )
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
