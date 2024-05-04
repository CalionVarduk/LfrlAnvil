using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Constructs.String;

/// <summary>
/// Represents a <see cref="BigInteger"/> to <see cref="String"/> type converter construct.
/// </summary>
public sealed class ParsedExpressionBigIntToStringTypeConverter : ParsedExpressionTypeConverter<string, BigInteger>
{
    private readonly MethodInfo _toString;
    private readonly ConstantExpression _formatProvider;

    /// <summary>
    /// Creates a new <see cref="ParsedExpressionBigIntToStringTypeConverter"/> instance.
    /// </summary>
    /// <param name="formatProvider">Optional format provider. Equal to null by default.</param>
    public ParsedExpressionBigIntToStringTypeConverter(IFormatProvider? formatProvider = null)
    {
        _formatProvider = Expression.Constant( formatProvider, typeof( IFormatProvider ) );
        _toString = MemberInfoLocator.FindToStringWithFormatProviderMethod( typeof( BigInteger ) );
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression? TryCreateFromConstant(ConstantExpression operand)
    {
        return TryGetSourceValue( operand, out var value )
            ? Expression.Constant( value.ToString( ReinterpretCast.To<IFormatProvider>( _formatProvider.Value ) ) )
            : null;
    }

    /// <inheritdoc />
    [Pure]
    protected override Expression CreateConversionExpression(Expression operand)
    {
        return Expression.Call( operand, _toString, _formatProvider );
    }
}
