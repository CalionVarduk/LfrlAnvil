using System;
using System.Linq.Expressions;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetMemberName<T, TMember>(this Expression<Func<T, TMember>> source)
        {
            var body = source.Body;
            Assert.True(
                body.NodeType == ExpressionType.MemberAccess,
                "Expression must be of the member access type." );

            var memberExpr = (MemberExpression) body;

            Assert.True(
                memberExpr.Expression == source.Parameters[0],
                "Member expression's target must be the same as the expression's parameter." );

            return memberExpr.Member.Name;
        }
    }
}
