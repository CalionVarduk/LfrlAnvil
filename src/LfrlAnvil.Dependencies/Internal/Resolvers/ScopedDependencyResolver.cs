using System;
using System.Linq.Expressions;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class ScopedDependencyResolver : DependencyResolver, IResolverFactorySource
{
    internal ScopedDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy )
    {
        Factory = expression.CreateResolverFactory( this );
    }

    public Func<DependencyScope, object> Factory { get; set; }
    internal override DependencyLifetime Lifetime => DependencyLifetime.Scoped;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        using ( ReadLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( scope ) ) );

            if ( scope.ScopedInstancesByResolverId.TryGetValue( Id, out var result ) )
                return result;
        }

        using ( WriteLockSlim.TryEnter( scope.Lock, out var entered ) )
        {
            if ( ! entered || scope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( Resources.ScopeIsDisposed( scope ) ) );

            return this.CreateScopedInstance( Factory, scope, dependencyType );
        }
    }
}
