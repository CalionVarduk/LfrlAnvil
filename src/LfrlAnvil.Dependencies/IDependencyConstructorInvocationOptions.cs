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

namespace LfrlAnvil.Dependencies;

/// <summary>
/// Represents custom dependency constructor invocation options.
/// </summary>
public interface IDependencyConstructorInvocationOptions
{
    /// <summary>
    /// Specifies an optional callback that gets invoked right after the dependency instance is created.
    /// The first argument is the resolved dependency, the second argument denotes the resolved dependency type
    /// and the third argument is the scope that was used to resolve the dependency.
    /// </summary>
    Action<object, Type, IDependencyScope>? OnCreatedCallback { get; }

    /// <summary>
    /// Contains all registered custom constructor parameter resolutions.
    /// </summary>
    IReadOnlyList<InjectableDependencyResolution<ParameterInfo>> ParameterResolutions { get; }

    /// <summary>
    /// Contains all registered custom injectable member resolutions.
    /// </summary>
    IReadOnlyList<InjectableDependencyResolution<MemberInfo>> MemberResolutions { get; }

    /// <summary>
    /// Sets the <see cref="OnCreatedCallback"/> for this instance.
    /// </summary>
    /// <param name="callback">Delegate to set.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions SetOnCreatedCallback(Action<object, Type, IDependencyScope>? callback);

    /// <summary>
    /// Registers a custom constructor parameter resolution based on a factory.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired constructor parameter, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="factory">Custom parameter factory.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory);

    /// <summary>
    /// Registers a custom constructor parameter resolution based on an explicit implementor type.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired constructor parameter, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="implementorType">Explicit implementor type.</param>
    /// <param name="configuration">Optional implementor configurator.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions ResolveParameter(
        Func<ParameterInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null);

    /// <summary>
    /// Registers a custom injectable member resolution based on a factory.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired injectable member, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="factory">Custom member factory.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Expression<Func<IDependencyScope, object>> factory);

    /// <summary>
    /// Registers a custom injectable member resolution based on an explicit implementor type.
    /// </summary>
    /// <param name="predicate">
    /// Predicate used for locating the desired injectable member, denoted by the predicate returning <b>true</b>.
    /// </param>
    /// <param name="implementorType">Explicit implementor type.</param>
    /// <param name="configuration">Optional implementor configurator.</param>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions ResolveMember(
        Func<MemberInfo, bool> predicate,
        Type implementorType,
        Action<IDependencyImplementorOptions>? configuration = null);

    /// <summary>
    /// Removes all registered custom constructor parameter resolutions from this instance.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions ClearParameterResolutions();

    /// <summary>
    /// Removes all registered custom injectable member resolutions from this instance.
    /// </summary>
    /// <returns><b>this</b>.</returns>
    IDependencyConstructorInvocationOptions ClearMemberResolutions();
}
