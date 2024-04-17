using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class CycleTrackingScopedSingletonDependencyResolver : CycleTrackingDependencyResolver, IResolverFactorySource
{
    internal CycleTrackingScopedSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = factory;
    }

    internal CycleTrackingScopedSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = expression.CreateResolverFactory( this );
    }

    public Func<DependencyScope, object> Factory { get; set; }
    internal override DependencyLifetime Lifetime => DependencyLifetime.ScopedSingleton;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        object? cached = null;
        using ( ReadLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );

            if ( scope.ScopedInstancesByResolverId.TryGetValue( Id, out var result ) )
                cached = result;
        }

        if ( cached is not null )
        {
            TryInvokeOnResolvingCallbackWithCycleTracking( dependencyType, scope );
            return cached;
        }

        cached = this.TryFindAncestorScopedSingletonInstance( scope.InternalParentScope );
        if ( cached is not null )
        {
            using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
            {
                if ( ! entered || scope.IsDisposed )
                    ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );

                ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( scope.ScopedInstancesByResolverId, Id, out var exists )!;
                if ( ! exists )
                    result = cached;
                else
                    cached = result;
            }

            TryInvokeOnResolvingCallbackWithCycleTracking( dependencyType, scope );
            return cached;
        }

        using ( TrackCycles( dependencyType ) )
        {
            TryInvokeOnResolvingCallback( dependencyType, scope );

            using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
            {
                if ( ! entered || scope.IsDisposed )
                    ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( scope ) ) );

                return this.CreateScopedInstance( Factory, scope, dependencyType );
            }
        }
    }
}
