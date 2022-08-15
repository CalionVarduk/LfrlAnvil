using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderMemberError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderMemberError(
        ParsedExpressionBuilderErrorType type,
        StringSlice token,
        Type targetType,
        MemberInfo member,
        Exception exception)
        : base( type, token )
    {
        TargetType = targetType;
        Member = member;
        Exception = exception;
    }

    public Type TargetType { get; }
    public MemberInfo Member { get; }
    public Exception Exception { get; }

    [Pure]
    public override string ToString()
    {
        var memberTypeText = Member is FieldInfo ? "(Field)" : "(Property)";
        var memberText = $"{Member.Name} {memberTypeText}";

        return
            $"{base.ToString()}, target type: {TargetType.FullName}, member: {memberText}, an exception has been thrown:{Environment.NewLine}{Exception}";
    }
}
