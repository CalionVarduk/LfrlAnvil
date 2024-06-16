// Copyright 2024 Łukasz Furlepa
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
using System.Linq.Expressions;
using LfrlAnvil.Async;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal sealed class CycleTrackingSingletonDependencyResolver : CycleTrackingDependencyResolver
{
    private Func<DependencyScope, object>? _factory;
    private object? _instance;

    internal CycleTrackingSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        _instance = null;
        _factory = factory;
    }

    internal CycleTrackingSingletonDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Expression<Func<DependencyScope, object>> expression)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
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

        object? cached = null;
        using ( ReadLockSlim.TryEnter( rootScope.Lock, out var entered ) )
        {
            if ( ! entered || rootScope.IsDisposed )
                ExceptionThrower.Throw( new ObjectDisposedException( null, Resources.ScopeIsDisposed( rootScope ) ) );

            if ( _instance is not null )
                cached = _instance;
        }

        if ( cached is not null )
        {
            TryInvokeOnResolvingCallbackWithCycleTracking( dependencyType, scope );
            return cached;
        }

        using ( TrackCycles( dependencyType ) )
        {
            TryInvokeOnResolvingCallback( dependencyType, scope );

            using var @lock = UpgradeableReadLockSlim.TryEnter( rootScope.Lock, out var entered );
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
