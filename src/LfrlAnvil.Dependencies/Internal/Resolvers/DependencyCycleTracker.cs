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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Dependencies.Exceptions;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Dependencies.Internal.Resolvers;

internal readonly struct DependencyCycleTracker : IDisposable
{
    [ThreadStatic]
    private static HashSet<ulong>? _activeResolverIds;

    private static HashSet<ulong> ActiveResolverIds => _activeResolverIds ??= new HashSet<ulong>();

    private readonly ulong _resolverId;

    private DependencyCycleTracker(ulong resolverId)
    {
        _resolverId = resolverId;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static DependencyCycleTracker Create(DependencyResolver resolver, Type dependencyType)
    {
        var resolverId = resolver.Id;
        if ( ! ActiveResolverIds.Add( resolverId ) )
            ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, resolver.ImplementorType ) );

        return new DependencyCycleTracker( resolverId );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        ActiveResolverIds.Remove( _resolverId );
    }
}
