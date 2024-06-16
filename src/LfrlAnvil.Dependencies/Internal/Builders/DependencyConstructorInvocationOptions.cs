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
using System.Linq.Expressions;
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyConstructorInvocationOptions : IDependencyConstructorInvocationOptions
{
    private readonly List<InjectableDependencyResolution<ParameterInfo>> _parameterResolutions;
    private readonly List<InjectableDependencyResolution<MemberInfo>> _memberResolutions;

    internal DependencyConstructorInvocationOptions(DependencyLocatorBuilder locatorBuilder)
    {
        OnCreatedCallback = null;
        _parameterResolutions = new List<InjectableDependencyResolution<ParameterInfo>>();
        _memberResolutions = new List<InjectableDependencyResolution<MemberInfo>>();
        LocatorBuilder = locatorBuilder;
    }

    public Action<object, Type, IDependencyScope>? OnCreatedCallback { get; private set; }
    public IReadOnlyList<InjectableDependencyResolution<ParameterInfo>> ParameterResolutions => _parameterResolutions;
    public IReadOnlyList<InjectableDependencyResolution<MemberInfo>> MemberResolutions => _memberResolutions;
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback)
    {
        OnCreatedCallback = callback;
        return this;
    }

    public IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory)
    {
        var resolution = InjectableDependencyResolution<ParameterInfo>.FromFactory( predicate, factory );
        _parameterResolutions.Add( resolution );
        return this;
    }

    public IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        var key = DependencyImplementorOptions.CreateImplementorKey(
            LocatorBuilder.CreateImplementorKey( implementorType ),
            configuration );

        var resolution = InjectableDependencyResolution<ParameterInfo>.FromImplementorKey( predicate, key );
        _parameterResolutions.Add( resolution );
        return this;
    }

    public IDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory)
    {
        var resolution = InjectableDependencyResolution<MemberInfo>.FromFactory( predicate, factory );
        _memberResolutions.Add( resolution );
        return this;
    }

    public IDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        var key = DependencyImplementorOptions.CreateImplementorKey(
            LocatorBuilder.CreateImplementorKey( implementorType ),
            configuration );

        var resolution = InjectableDependencyResolution<MemberInfo>.FromImplementorKey( predicate, key );
        _memberResolutions.Add( resolution );
        return this;
    }

    public IDependencyConstructorInvocationOptions ClearParameterResolutions()
    {
        _parameterResolutions.Clear();
        return this;
    }

    public IDependencyConstructorInvocationOptions ClearMemberResolutions()
    {
        _memberResolutions.Clear();
        return this;
    }
}
