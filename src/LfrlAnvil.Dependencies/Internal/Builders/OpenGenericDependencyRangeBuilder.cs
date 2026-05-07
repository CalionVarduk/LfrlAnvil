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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class OpenGenericDependencyRangeBuilder : IOpenGenericDependencyRangeBuilder, IInternalDependencyRangeBuilder
{
    private readonly List<DependencyRangeBuilder> _closedBuilders = new List<DependencyRangeBuilder>();

    internal OpenGenericDependencyRangeBuilder(DependencyLocatorBuilder locatorBuilder, Type dependencyType)
    {
        InternalElements = new List<OpenGenericDependencyBuilder>();
        DependencyType = dependencyType;
        LocatorBuilder = locatorBuilder;
        OnResolvingCallback = null;
    }

    public Type DependencyType { get; }
    public Action<Type, IDependencyScope>? OnResolvingCallback { get; private set; }
    public bool IsOpenGeneric => true;
    public IReadOnlyList<IOpenGenericDependencyBuilder> Elements => InternalElements;
    public IReadOnlyList<IDependencyRangeBuilder> ClosedBuilders => _closedBuilders;
    internal List<OpenGenericDependencyBuilder> InternalElements { get; }
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IOpenGenericDependencyBuilder Add()
    {
        var result = new OpenGenericDependencyBuilder( this );
        InternalElements.Add( result );

        foreach ( var b in _closedBuilders )
            b.InternalElements.Add( result );

        return result;
    }

    [Pure]
    public IOpenGenericDependencyBuilder? TryGetLast()
    {
        return InternalElements.Count > 0 ? InternalElements[^1] : null;
    }

    public IOpenGenericDependencyRangeBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback)
    {
        OnResolvingCallback = callback;
        return this;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void AddClosedBuilder(DependencyRangeBuilder builder)
    {
        Assume.True( builder.DependencyType.IsGenericType && builder.DependencyType.GetGenericTypeDefinition() == DependencyType );
        Assume.False( _closedBuilders.Contains( builder ) );

        _closedBuilders.Add( builder );
        foreach ( var b in InternalElements )
            builder.InternalElements.Add( b );
    }
}
