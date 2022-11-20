using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Internal.Resolvers;
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal readonly struct DependencyLocatorBuilderParams
{
    internal readonly DependencyLocatorBuilderStore LocatorBuilderStore;
    internal readonly KeyedDependencyResolversStore KeyedResolversStore;

    private readonly Dictionary<Type, DependencyResolver> _defaultResolvers;
    private readonly Dictionary<(ISharedDependencyImplementorKey, DependencyLifetime), DependencyResolver> _sharedResolvers;
    private readonly UlongSequenceGenerator _resolverIdGenerator;

    private DependencyLocatorBuilderParams(DependencyLocatorBuilderStore locatorBuilderStore)
    {
        LocatorBuilderStore = locatorBuilderStore;
        _resolverIdGenerator = new UlongSequenceGenerator();
        _sharedResolvers = new Dictionary<(ISharedDependencyImplementorKey, DependencyLifetime), DependencyResolver>();

        _defaultResolvers = new Dictionary<Type, DependencyResolver>
        {
            { typeof( IDependencyContainer ), new DependencyContainerResolver( _resolverIdGenerator.Generate() ) },
            { typeof( IDependencyScope ), new DependencyScopeResolver( _resolverIdGenerator.Generate() ) }
        };

        KeyedResolversStore = KeyedDependencyResolversStore.Create( _defaultResolvers );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetResolverId()
    {
        return _resolverIdGenerator.Generate();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DependencyResolver? GetSharedResolver(ISharedDependencyImplementorKey key, DependencyLifetime lifetime)
    {
        return _sharedResolvers.GetValueOrDefault( (key, lifetime) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddSharedResolver(ISharedDependencyImplementorKey key, DependencyLifetime lifetime, DependencyResolver resolver)
    {
        _sharedResolvers.Add( (key, lifetime), resolver );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal IReadOnlyDictionary<Type, DependencyResolver> GetDefaultResolvers()
    {
        return _defaultResolvers;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyLocatorBuilderParams Create(DependencyLocatorBuilderStore locatorBuilderStore)
    {
        return new DependencyLocatorBuilderParams( locatorBuilderStore );
    }
}
