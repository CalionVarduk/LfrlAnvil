﻿using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class TransientDependencyResolver : FactoryDependencyResolver
{
    internal TransientDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, factory ) { }

    internal TransientDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback, expression ) { }

    internal override DependencyLifetime Lifetime => DependencyLifetime.Transient;

    protected override object CreateInternal(DependencyScope scope)
    {
        Assume.IsNotNull( Factory, nameof( Factory ) );
        var result = Factory( scope );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}
