using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public class ParsedExpressionBuilderAmbiguousMemberAccessError : ParsedExpressionBuilderError
{
    internal ParsedExpressionBuilderAmbiguousMemberAccessError(
        ParsedExpressionBuilderErrorType type,
        StringSlice token,
        Type targetType,
        IReadOnlyList<MemberInfo> members)
        : base( type, token )
    {
        TargetType = targetType;
        Members = members;
    }

    public Type TargetType { get; }
    public IReadOnlyList<MemberInfo> Members { get; }

    [Pure]
    public override string ToString()
    {
        var membersText = string.Join(
            Environment.NewLine,
            Members.Select(
                (m, i) =>
                {
                    var typeText = m is FieldInfo ? "(Field)" : "(Property)";
                    return $"{i + 1}. {m.Name} {typeText}";
                } ) );

        return $"{base.ToString()}, target type: {TargetType.FullName}, found {Members.Count} members:{Environment.NewLine}{membersText}";
    }
}
