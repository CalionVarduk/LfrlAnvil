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
/// Represents an error that occurred due to ambiguous member name.
/// </summary>
public class ParsedExpressionMemberAmbiguityException : InvalidOperationException
{
    /// <summary>
    /// Creates a new <see cref="ParsedExpressionMemberAmbiguityException"/> instance.
    /// </summary>
    /// <param name="targetType">Target type.</param>
    /// <param name="memberName">Member name.</param>
    /// <param name="members">Collection of found members.</param>
    public ParsedExpressionMemberAmbiguityException(Type targetType, string memberName, IReadOnlyList<MemberInfo> members)
        : base( Resources.AmbiguousMembers( targetType, memberName, members ) )
    {
        TargetType = targetType;
        MemberName = memberName;
        Members = members;
    }

    /// <summary>
    /// Target type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Member name.
    /// </summary>
    public string MemberName { get; }

    /// <summary>
    /// Collection of found members.
    /// </summary>
    public IReadOnlyList<MemberInfo> Members { get; }
}
