using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Errors;

public sealed class ParsedExpressionBuilderAmbiguousMemberAccessError : ParsedExpressionBuilderError
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
                    if ( m is MethodInfo method )
                    {
                        var genericArgs = method.GetGenericArguments();
                        var parameters = method.GetParameters();

                        var fullName = method.Name;
                        if ( genericArgs.Length > 0 )
                            fullName += $"[{string.Join( ", ", genericArgs.Select( t => t.FullName ) )}]";

                        fullName += $"({string.Join( ", ", parameters.Select( p => $"{p.ParameterType.FullName} {p.Name}" ) )})";
                        return $"{i + 1}. {fullName} (Method)";
                    }

                    var typeText = m is FieldInfo ? "(Field)" : "(Property)";
                    return $"{i + 1}. {m.Name} {typeText}";
                } ) );

        return $"{base.ToString()}, target type: {TargetType.FullName}, found {Members.Count} members:{Environment.NewLine}{membersText}";
    }
}
