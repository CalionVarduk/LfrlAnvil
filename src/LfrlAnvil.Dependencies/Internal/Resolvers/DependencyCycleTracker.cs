using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal readonly struct DependencyCycleTracker : IDisposable
{
    [ThreadStatic]
    private static HashSet<ulong>? _activeResolverIds;

    private static HashSet<ulong> ActiveResolverIds => _activeResolverIds ??= new HashSet<ulong>();

    private readonly ulong _resolverId;

    private DependencyCycleTracker(ulong resolverId)
    {
        _resolverId = resolverId;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyCycleTracker Create(DependencyResolver resolver, Type dependencyType)
    {
        var resolverId = resolver.Id;
        if ( ! ActiveResolverIds.Add( resolverId ) )
            ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, resolver.ImplementorType ) );

        return new DependencyCycleTracker( resolverId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        ActiveResolverIds.Remove( _resolverId );
    }
}
