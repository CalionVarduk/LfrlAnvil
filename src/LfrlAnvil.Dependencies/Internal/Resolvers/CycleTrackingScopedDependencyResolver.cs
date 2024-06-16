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

internal sealed class CycleTrackingScopedDependencyResolver : CycleTrackingDependencyResolver, IResolverFactorySource
{
    internal CycleTrackingScopedDependencyResolver(
        ulong id,
        Type implementorType,
        DependencyImplementorDisposalStrategy disposalStrategy,
        Action<Type, IDependencyScope>? onResolvingCallback,
        Func<IDependencyScope, object> factory)
        : base( id, implementorType, disposalStrategy, onResolvingCallback )
    {
        Factory = factory;
    }

    internal CycleTrackingScopedDependencyResolver(
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
    internal override DependencyLifetime Lifetime => DependencyLifetime.Scoped;

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
