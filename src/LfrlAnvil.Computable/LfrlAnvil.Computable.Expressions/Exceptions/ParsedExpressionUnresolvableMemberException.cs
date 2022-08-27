using System;
using System.Reflection;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionUnresolvableMemberException : InvalidOperationException
{
    public ParsedExpressionUnresolvableMemberException(Type targetType, MemberTypes memberType, string memberName)
        : base( Resources.UnresolvableMember( targetType, memberType, memberName ) )
    {
        TargetType = targetType;
        MemberType = memberType;
        MemberName = memberName;
    }

    public Type TargetType { get; }
    public MemberTypes MemberType { get; }
    public string MemberName { get; }
}
