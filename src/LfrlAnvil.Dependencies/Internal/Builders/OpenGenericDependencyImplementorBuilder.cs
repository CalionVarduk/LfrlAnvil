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
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class OpenGenericDependencyImplementorBuilder : IOpenGenericDependencyImplementorBuilder
{
    internal OpenGenericDependencyImplementorBuilder(DependencyLocatorBuilder locatorBuilder, Type implementorType)
    {
        LocatorBuilder = locatorBuilder;
        ImplementorType = implementorType;
        DisposalStrategy = locatorBuilder.DefaultDisposalStrategy;
        OnResolvingCallback = null;
        InternalConstructor = null;
    }

    public Type ImplementorType { get; }
    public DependencyImplementorDisposalStrategy DisposalStrategy { get; private set; }
    public Action<Type, IDependencyScope>? OnResolvingCallback { get; private set; }
    public IDependencyConstructor? Constructor => InternalConstructor;

    internal DependencyLocatorBuilder LocatorBuilder { get; }
    internal DependencyConstructor? InternalConstructor { get; private set; }

    public IOpenGenericDependencyImplementorBuilder FromConstructor(Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return FromConstructorCore( null, configuration );
    }

    public IOpenGenericDependencyImplementorBuilder FromConstructor(
        ConstructorInfo info,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        return FromConstructorCore( info, configuration );
    }

    public IOpenGenericDependencyImplementorBuilder FromType(
        Type type,
        Action<IDependencyConstructorInvocationOptions>? configuration = null)
    {
        InternalConstructor = new DependencyConstructor( LocatorBuilder, type );
        configuration?.Invoke( InternalConstructor.InternalInvocationOptions );
        return this;
    }

    public IOpenGenericDependencyImplementorBuilder SetDisposalStrategy(DependencyImplementorDisposalStrategy strategy)
    {
        DisposalStrategy = strategy;
        return this;
    }

    public IOpenGenericDependencyImplementorBuilder SetOnResolvingCallback(Action<Type, IDependencyScope>? callback)
    {
        OnResolvingCallback = callback;
        return this;
    }

    private OpenGenericDependencyImplementorBuilder FromConstructorCore(
        ConstructorInfo? info,
        Action<IDependencyConstructorInvocationOptions>? configuration)
    {
        InternalConstructor = new DependencyConstructor( LocatorBuilder, info );
        configuration?.Invoke( InternalConstructor.InternalInvocationOptions );
        return this;
    }
}
