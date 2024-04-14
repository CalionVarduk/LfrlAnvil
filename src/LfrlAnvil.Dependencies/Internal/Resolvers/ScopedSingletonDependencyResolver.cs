﻿using System;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedSingletonDependencyResolver : DependencyResolver, IResolverFactorySource
{
    internal ScopedSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy )
    {
        Factory = expression.CreateResolverFactory( this );
    }

    public Func<DependencyScope, object> Factory { get; set; }
    internal override DependencyLifetime Lifetime => DependencyLifetime.ScopedSingleton;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        using ( ReadLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( scope ) ) );

            if ( scope.ScopedInstancesByResolverId.TryGetValue( Id, out var result ) )
                return result;
        }

        var ancestorResult = this.TryFindAncestorScopedSingletonInstance( scope.InternalParentScope );
        if ( ancestorResult is not null )
        {
            using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
            {
                if ( ! entered || scope.IsDisposed )
                    ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( scope ) ) );

                ref var result = ref CollectionsMarshal.GetValueRefOrAddDefault( scope.ScopedInstancesByResolverId, Id, out var exists )!;
                if ( exists )
                    return result;

                result = ancestorResult;
            }

            return ancestorResult;
        }

        using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( scope ) ) );

            return this.CreateScopedInstance( Factory, scope, dependencyType );
        }
    }
}
