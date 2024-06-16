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

internal abstract class DependencyResolver
{
    protected DependencyResolver(ulong id, Type implementorType, DependencyImplementorDisposalStrategy disposalStrategy)
    {
        Id = id;
        ImplementorType = implementorType;
        DisposalStrategy = disposalStrategy;
    }

    internal ulong Id { get; }
    internal Type ImplementorType { get; }
    internal abstract DependencyLifetime Lifetime { get; }
    internal DependencyImplementorDisposalStrategy DisposalStrategy { get; }

    internal abstract object Create(DependencyScope scope, Type dependencyType);

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal object InvokeFactory(Func<DependencyScope, object> factory, DependencyScope scope, Type dependencyType)
    {
        try
        {
            return factory( scope );
        }
        catch ( CircularDependencyReferenceException exc )
        {
            ExceptionThrower.Throw( new CircularDependencyReferenceException( dependencyType, ImplementorType, exc ) );
            return default!;
        }
    }
}
