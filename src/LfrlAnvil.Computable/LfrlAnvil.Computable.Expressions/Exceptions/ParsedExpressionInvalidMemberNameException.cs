using System;
using System.Linq.Expressions;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

public class ParsedExpressionInvalidMemberNameException : InvalidOperationException
{
    public ParsedExpressionInvalidMemberNameException(Expression expression)
        : base( Resources.MemberNameMustBeConstantNonNullString )
    {
        Expression = expression;
    }

    public Expression Expression { get; }
}
