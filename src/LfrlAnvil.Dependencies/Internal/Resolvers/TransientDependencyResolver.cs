﻿using System;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class TransientDependencyResolver : DependencyResolver
{
    private readonly Func<IDependencyScope, object> _factory;

    internal TransientDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        _factory = factory;
    }

    protected override object CreateInternal(DependencyScope scope)
    {
        var result = _factory( scope );
        SetupDisposalStrategy( scope, result );
        return result;
    }
}