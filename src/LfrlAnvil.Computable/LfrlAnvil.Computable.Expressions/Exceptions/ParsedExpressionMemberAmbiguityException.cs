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
