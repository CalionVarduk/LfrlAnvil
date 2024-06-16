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

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyRangeBuilder : IDependencyRangeBuilder
{
    internal DependencyRangeBuilder(DependencyLocatorBuilder locatorBuilder, Type dependencyType)
    {
        InternalElements = new List<DependencyBuilder>();
        DependencyType = dependencyType;
        LocatorBuilder = locatorBuilder;
        OnResolvingCallback = null;
    }

    public Type DependencyType { get; }
    public Action<Type, IDependencyScope>? OnResolvingCallback { get; private set; }
    public IReadOnlyList<IDependencyBuilder> Elements => InternalElements;
    internal List<DependencyBuilder> InternalElements { get; }
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IDependencyBuilder Add()
    {
        var result = new DependencyBuilder( this );
        InternalElements.Add( result );
        return result;
    }

    [Pure]
    public IDependencyBuilder? TryGetLast()
    {
        return InternalElements.Count > 0 ? InternalElements[^1] : null;
    }

    public IDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback)
    {
        OnResolvingCallback = callback;
        return this;
    }
}
