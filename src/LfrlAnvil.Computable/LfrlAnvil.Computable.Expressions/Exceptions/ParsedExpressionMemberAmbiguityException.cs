using System;
using System.Collections.Generic;
using System.Reflection;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionMemberAmbiguityException : InvalidOperationException
{
    public ParsedExpressionMemberAmbiguityException(Type targetType, string memberName, IReadOnlyList<MemberInfo> members)
        : base( Resources.AmbiguousMembers( targetType, memberName, members ) )
    {
        TargetType = targetType;
        MemberName = memberName;
        Members = members;
    }

    public Type TargetType { get; }
    public string MemberName { get; }
    public IReadOnlyList<MemberInfo> Members { get; }
}
