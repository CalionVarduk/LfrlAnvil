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
using System.Diagnostics.Contracts;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class OpenGenericDependencyBuilder : IOpenGenericDependencyBuilder, IInternalDependencyBuilder
{
    internal OpenGenericDependencyBuilder(OpenGenericDependencyRangeBuilder rangeBuilder)
    {
        InternalRangeBuilder = rangeBuilder;
        Lifetime = rangeBuilder.LocatorBuilder.DefaultLifetime;
        InternalSharedImplementorKey = null;
        Implementor = null;
        IsIncludedInRange = true;
    }

    public DependencyLifetime Lifetime { get; private set; }
    public IOpenGenericDependencyImplementorBuilder? Implementor { get; private set; }
    public bool IsIncludedInRange { get; private set; }
    public bool IsLastInRange => ReferenceEquals( this, InternalRangeBuilder.TryGetLast() );
    public int RangeIndex => InternalRangeBuilder.InternalElements.IndexOf( this );
    public bool IsOpenGeneric => true;
    public Type DependencyType => InternalRangeBuilder.DependencyType;
    public IDependencyKey? SharedImplementorKey => InternalSharedImplementorKey;
    public IOpenGenericDependencyRangeBuilder RangeBuilder => InternalRangeBuilder;
    internal OpenGenericDependencyRangeBuilder InternalRangeBuilder { get; }
    internal IInternalDependencyKey? InternalSharedImplementorKey { get; private set; }

    public IOpenGenericDependencyBuilder IncludeInRange(bool included = true)
    {
        IsIncludedInRange = included;
        return this;
    }

    [Pure]
    public bool IsLastInClosedRange(IDependencyRangeBuilder builder)
    {
        return builder is DependencyRangeBuilder b && b.InternalElements.Count > 0 && ReferenceEquals( this, b.InternalElements[^1] );
    }

    [Pure]
    public int GetClosedRangeIndex(IDependencyRangeBuilder builder)
    {
        return builder is DependencyRangeBuilder b ? b.InternalElements.IndexOf( this ) : -1;
    }

    public IOpenGenericDependencyBuilder SetLifetime(DependencyLifetime lifetime)
    {
        Ensure.IsDefined( lifetime );
        Lifetime = lifetime;
        return this;
    }

    public IOpenGenericDependencyBuilder FromSharedImplementor(Type type, Action<IDependencyImplementorOptions>? configuration = null)
    {
        InternalSharedImplementorKey = DependencyImplementorOptions.CreateImplementorKey(
            InternalRangeBuilder.LocatorBuilder.CreateImplementorKey( type ),
            configuration );

        Implementor = null;
        return this;
    }

    public IOpenGenericDependencyImplementorBuilder FromConstructor(
        Action<IOpenGenericDependencyConstructorInvocationOptions>? configuration = null)
    {
        return GetOrCreateImplementor().FromConstructor( configuration );
    }

    public IOpenGenericDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IOpenGenericDependencyConstructorInvocationOptions>? configuration = null)
    {
        return GetOrCreateImplementor().FromConstructor( info, configuration );
    }

    public IOpenGenericDependencyImplementorBuilder FromType(
        Type type,
        Action<IOpenGenericDependencyConstructorInvocationOptions>? configuration = null)
    {
        return GetOrCreateImplementor().FromType( type, configuration );
    }

    private IOpenGenericDependencyImplementorBuilder GetOrCreateImplementor()
    {
        if ( Implementor is not null )
            return Implementor;

        InternalSharedImplementorKey = null;
        Implementor = new OpenGenericDependencyImplementorBuilder( InternalRangeBuilder.LocatorBuilder, DependencyType );
        return Implementor;
    }
}
