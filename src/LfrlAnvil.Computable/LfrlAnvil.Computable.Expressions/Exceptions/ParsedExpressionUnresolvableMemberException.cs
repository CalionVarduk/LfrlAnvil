using System;
using System.Collections.Generic;
using System.Reflection;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionUnresolvableMemberException : InvalidOperationException
{
    public ParsedExpressionUnresolvableMemberException(Type targetType, string memberName)
        : base( Resources.UnresolvableMember( targetType, MemberTypes.Field | MemberTypes.Property, memberName, parameterTypes: null ) )
    {
        TargetType = targetType;
        MemberType = MemberTypes.Field | MemberTypes.Property;
        MemberName = memberName;
        ParameterTypes = null;
    }

    public ParsedExpressionUnresolvableMemberException(Type targetType, string methodName, IReadOnlyList<Type> parameterTypes)
        : base( Resources.UnresolvableMember( targetType, MemberTypes.Method, methodName, parameterTypes ) )
    {
        TargetType = targetType;
        MemberType = MemberTypes.Method;
        MemberName = methodName;
        ParameterTypes = parameterTypes;
    }

    public Type TargetType { get; }
    public MemberTypes MemberType { get; }
    public string MemberName { get; }
    public IReadOnlyList<Type>? ParameterTypes { get; }
}
