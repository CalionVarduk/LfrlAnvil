using System;
using System.Linq.Expressions;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class SingletonDependencyResolver : DependencyResolver
{
    private Func<DependencyScope, object>? _factory;
    private object? _instance;

    internal SingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy )
    {
        _instance = null;
        _factory = scope =>
        {
            var factory = expression.Compile();
            return factory( scope );
        };
    }

    internal override DependencyLifetime Lifetime => DependencyLifetime.Singleton;

    internal override object Create(DependencyScope scope, Type dependencyType)
    {
        var rootScope = scope.InternalContainer.InternalRootScope;
        using ( ReadLockSlim.TryEnter( rootScope.Lock, out var entered ) )
        {
            if ( ! entered || rootScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( rootScope ) ) );

            if ( _instance is not null )
                return _instance;
        }

        using ( var @lock = UpgradeableReadLockSlim.TryEnter( rootScope.Lock, out var entered ) )
        {
            if ( ! entered || rootScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( rootScope ) ) );

            if ( _instance is not null )
                return _instance;

            using ( @lock.Upgrade() )
            {
                Assume.IsNotNull( _factory );
                _instance = InvokeFactory( _factory, scope, dependencyType );
                _factory = null;

                var disposer = DisposalStrategy.TryCreateDisposer( _instance );
                if ( disposer is not null )
                    rootScope.InternalDisposers.Add( disposer.Value );

                return _instance;
            }
        }
    }
}
