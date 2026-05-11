// Copyright 2026 Łukasz Furlepa
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
using LfrlAnvil.Generators;

namespace LfrlAnvil.Dependencies.Internal.Resolvers.Factories;

internal sealed class ClosedGenericScopedSingletonDependencyResolverFactory : RegisteredClosedGenericDependencyResolverFactory
{
    internal ClosedGenericScopedSingletonDependencyResolverFactory(
        ImplementorKey implementorKey,
        OpenGenericDependencyResolverFactory @base)
        : base( implementorKey, @base ) { }

    protected override DependencyResolver CreateFromExpression(
        Expression<Func<DependencyScope, object>> expression,
        UlongSequenceGenerator idGenerator)
    {
        return Base.ImplementorBuilder.OnResolvingCallback is null
            && Base.ImplementorBuilder.Constructor?.InvocationOptions.OnCreatedCallback is null
                ? new ScopedSingletonDependencyResolver(
                    idGenerator.Generate(),
                    ImplementorKey.Value.Type,
                    DisposalStrategy,
                    expression )
                : new CycleTrackingScopedSingletonDependencyResolver(
                    idGenerator.Generate(),
                    ImplementorKey.Value.Type,
                    DisposalStrategy,
                    Base.ImplementorBuilder.OnResolvingCallback,
                    expression );
    }
}
