using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

/// <summary>
/// Represents a type converter construct to <see cref="String"/> type.
/// </summary>
public sealed class ParsedExpressionToStringTypeConverter : ParsedExpressionTypeConverter<string>
{
    private readonly MethodInfo _toString;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionToStringTypeConverter"/> instance.
    /// </summary>
    public ParsedExpressionToStringTypeConverter()
    {
        _toString = MemberInfoLocator.FindToStringMethod();
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression TryCreateFromConstant(ConstantExpression operand)
    {
        Ensure.IsNotNull( operand.Value );
        return Expression.Constant( operand.Value.ToString() );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString );
    }
}
