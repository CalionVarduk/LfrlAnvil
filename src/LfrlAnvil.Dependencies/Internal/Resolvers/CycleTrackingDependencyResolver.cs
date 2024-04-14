using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal abstract class CycleTrackingDependencyResolver : DependencyResolver
{
    protected CycleTrackingDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback)
        : base( id, implementorType, disposalStrategy )
    {
        OnResolvingCallback = onResolvingCallback;
    }

    internal Action<Type, IDependencyScope>? OnResolvingCallback { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected DependencyCycleTracker TrackCycles(Type dependencyType)
    {
        return DependencyCycleTracker.Create( this, dependencyType );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void TryInvokeOnResolvingCallbackWithCycleTracking(Type dependencyType, DependencyScope scope)
    {
        if ( OnResolvingCallback is null )
            return;

        using ( TrackCycles( dependencyType ) )
        {
            try
            {
                OnResolvingCallback( dependencyType, scope );
            }
            catch ( CircularDependencyReferenceException exc )
            {
                ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, ImplementorType, exc ) );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    protected void TryInvokeOnResolvingCallback(Type dependencyType, DependencyScope scope)
    {
        if ( OnResolvingCallback is null )
            return;

        try
        {
            OnResolvingCallback( dependencyType, scope );
        }
        catch ( CircularDependencyReferenceException exc )
        {
            ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, ImplementorType, exc ) );
        }
    }
}
