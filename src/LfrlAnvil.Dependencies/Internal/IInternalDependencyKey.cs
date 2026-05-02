// Copyright 2024-2026 Łukasz Furlepa
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
using LfrlAnvil.Dependencies.Internal.Builders;

namespace LfrlAnvil.Dependencies.Internal;

internal interface IInternalDependencyKey : IDependencyKey
{
    [Pure]
    DependencyImplementorBuilder? GetSharedImplementor(DependencyLocatorBuilderStore builderStore);

    [Pure]
    OpenGenericDependencyImplementorBuilder? GetSharedGenericImplementor(DependencyLocatorBuilderStore builderStore);

    [Pure]
    DependencyResolversStore GetTargetResolversStore(
        in DependencyResolversStore globalResolvers,
        in KeyedDependencyResolversStore keyedResolversStore);

    [Pure]
    DependencyResolversStore GetResolversStore(DependencyLocator dependencyLocator);

    [Pure]
    IInternalDependencyKey WithType(Type type);
}
