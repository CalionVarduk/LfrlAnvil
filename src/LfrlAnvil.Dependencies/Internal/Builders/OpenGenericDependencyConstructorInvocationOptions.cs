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
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class OpenGenericDependencyConstructorInvocationOptions : IOpenGenericDependencyConstructorInvocationOptions
{
    private readonly List<OpenGenericInjectableDependencyResolution<ParameterInfo>> _parameterResolutions;
    private readonly List<OpenGenericInjectableDependencyResolution<MemberInfo>> _memberResolutions;

    internal OpenGenericDependencyConstructorInvocationOptions(DependencyLocatorBuilder locatorBuilder)
    {
        OnCreatedCallback = null;
        _parameterResolutions = new List<OpenGenericInjectableDependencyResolution<ParameterInfo>>();
        _memberResolutions = new List<OpenGenericInjectableDependencyResolution<MemberInfo>>();
        LocatorBuilder = locatorBuilder;
    }

    public Action<object, Type, IDependencyScope>? OnCreatedCallback { get; private set; }
    public IReadOnlyList<OpenGenericInjectableDependencyResolution<ParameterInfo>> ParameterResolutions => _parameterResolutions;
    public IReadOnlyList<OpenGenericInjectableDependencyResolution<MemberInfo>> MemberResolutions => _memberResolutions;
    internal DependencyLocatorBuilder LocatorBuilder { get; }

    public IOpenGenericDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback)
    {
        OnCreatedCallback = callback;
        return this;
    }

    public IOpenGenericDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        var key = DependencyImplementorOptions.CreateImplementorKey(
            LocatorBuilder.CreateImplementorKey( implementorType ),
            configuration );

        var resolution = OpenGenericInjectableDependencyResolution<ParameterInfo>.FromImplementorKey( predicate, key );
        _parameterResolutions.Add( resolution );
        return this;
    }

    public IOpenGenericDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null)
    {
        var key = DependencyImplementorOptions.CreateImplementorKey(
            LocatorBuilder.CreateImplementorKey( implementorType ),
            configuration );

        var resolution = OpenGenericInjectableDependencyResolution<MemberInfo>.FromImplementorKey( predicate, key );
        _memberResolutions.Add( resolution );
        return this;
    }

    public IOpenGenericDependencyConstructorInvocationOptions ClearParameterResolutions()
    {
        _parameterResolutions.Clear();
        return this;
    }

    public IOpenGenericDependencyConstructorInvocationOptions ClearMemberResolutions()
    {
        _memberResolutions.Clear();
        return this;
    }
}
