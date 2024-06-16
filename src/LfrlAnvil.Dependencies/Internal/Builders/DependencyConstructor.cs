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
using System.Reflection;

namespace LfrlAnvil.Dependencies.Internal.Builders;

internal sealed class DependencyConstructor : IDependencyConstructor
{
    private readonly Type? _type;

    internal DependencyConstructor(DependencyLocatorBuilder locatorBuilder, ConstructorInfo? info)
    {
        _type = null;
        Info = info;
        InternalInvocationOptions = new DependencyConstructorInvocationOptions( locatorBuilder );
    }

    internal DependencyConstructor(DependencyLocatorBuilder locatorBuilder, Type type)
    {
        _type = type;
        Info = null;
        InternalInvocationOptions = new DependencyConstructorInvocationOptions( locatorBuilder );
    }

    public ConstructorInfo? Info { get; }
    public Type? Type => Info?.DeclaringType ?? _type;
    public IDependencyConstructorInvocationOptions InvocationOptions => InternalInvocationOptions;
    internal DependencyConstructorInvocationOptions InternalInvocationOptions { get; }
}
