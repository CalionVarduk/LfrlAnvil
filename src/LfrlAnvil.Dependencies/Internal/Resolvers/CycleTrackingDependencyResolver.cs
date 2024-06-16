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
