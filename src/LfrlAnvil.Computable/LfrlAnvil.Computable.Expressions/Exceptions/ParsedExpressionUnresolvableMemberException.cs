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
using System.Reflection;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

/// <summary>
/// Represents an error that occurred due to a missing member.
/// </summary>
public class ParsedExpressionUnresolvableMemberException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnresolvableMemberException"/> instance.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="memberName">Field or property name.</param>
    public ParsedExpressionUnresolvableMemberException(Type targetType, string memberName)
        : base( Resources.UnresolvableMember( targetType, MemberTypes.Field | MemberTypes.Property, memberName, parameterTypes: null ) )
    {
        TargetType = targetType;
        MemberType = MemberTypes.Field | MemberTypes.Property;
        MemberName = memberName;
        ParameterTypes = null;
    }

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionUnresolvableMemberException"/> instance.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="methodName">Method name.</param>
    /// <param name="parameterTypes">Parameter types.</param>
    public ParsedExpressionUnresolvableMemberException(Type targetType, string methodName, IReadOnlyList<Type> parameterTypes)
        : base( Resources.UnresolvableMember( targetType, MemberTypes.Method, methodName, parameterTypes ) )
    {
        TargetType = targetType;
        MemberType = MemberTypes.Method;
        MemberName = methodName;
        ParameterTypes = parameterTypes;
    }

    /// <summary>
    /// Target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Type of the missing member.
    /// </summary>
    public MemberTypes MemberType { get; }

    /// <summary>
    /// Member name.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Parameter types.
    /// </summary>
    public IReadOnlyList<Type>? ParameterTypes { get; }
}
