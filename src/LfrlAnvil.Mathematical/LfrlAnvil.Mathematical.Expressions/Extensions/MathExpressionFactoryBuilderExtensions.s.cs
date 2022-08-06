using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LfrlAnvil.Extensions;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Constructs.BigInt;
using LfrlAnvil.Mathematical.Expressions.Constructs.Boolean;
using LfrlAnvil.Mathematical.Expressions.Constructs.Decimal;
using LfrlAnvil.Mathematical.Expressions.Constructs.Double;
using LfrlAnvil.Mathematical.Expressions.Constructs.Float;
using LfrlAnvil.Mathematical.Expressions.Constructs.Int32;
using LfrlAnvil.Mathematical.Expressions.Constructs.Int64;
using LfrlAnvil.Mathematical.Expressions.Constructs.String;

namespace LfrlAnvil.Mathematical.Expressions.Extensions;

public static class MathExpressionFactoryBuilderExtensions
{
    public static MathExpressionFactoryBuilder AddGenericArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddGenericBitwiseOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseAndSymbol, new MathExpressionBitwiseAndOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseOrSymbol, new MathExpressionBitwiseOrOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseXorSymbol, new MathExpressionBitwiseXorOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseLeftShiftSymbol, new MathExpressionBitwiseLeftShiftOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseRightShiftSymbol, new MathExpressionBitwiseRightShiftOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.BitwiseNotSymbol, new MathExpressionBitwiseNotOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseAndSymbol,
                MathExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseOrSymbol,
                MathExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseXorSymbol,
                MathExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                MathExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                MathExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.BitwiseNotSymbol,
                MathExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    public static MathExpressionFactoryBuilder AddGenericLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.CoalesceSymbol, new MathExpressionCoalesceOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CoalesceSymbol,
                MathExpressionConstructDefaults.CoalescePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddBooleanBitwiseOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseAndSymbol, new MathExpressionBitwiseAndBooleanOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseOrSymbol, new MathExpressionBitwiseOrBooleanOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseXorSymbol, new MathExpressionBitwiseXorBooleanOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseAndSymbol,
                MathExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseOrSymbol,
                MathExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseXorSymbol,
                MathExpressionConstructDefaults.BitwiseXorPrecedence );
    }

    public static MathExpressionFactoryBuilder AddBooleanLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToBooleanOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToBooleanOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareBooleanOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.AndSymbol, new MathExpressionAndOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.OrSymbol, new MathExpressionOrOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NotSymbol, new MathExpressionNotOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AndSymbol,
                MathExpressionConstructDefaults.AndPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.OrSymbol,
                MathExpressionConstructDefaults.OrPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NotSymbol,
                MathExpressionConstructDefaults.NotPrecedence );
    }

    public static MathExpressionFactoryBuilder AddToBooleanTypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[boolean]",
        string? postfixSymbol = null)
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<bool>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToBooleanTypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[boolean]",
        string? postfixSymbol = null,
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToBooleanTypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddDecimalArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloDecimalOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateDecimalOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddDecimalLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanDecimalOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToDecimalOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToDecimalOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareDecimalOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToDecimalTypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[decimal]",
        string? postfixSymbol = "M")
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<decimal>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToDecimalTypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[decimal]",
        string? postfixSymbol = "M",
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToDecimalTypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddDoubleArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloDoubleOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateDoubleOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddDoubleLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanDoubleOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToDoubleOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToDoubleOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareDoubleOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToDoubleTypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[double]",
        string? postfixSymbol = null)
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<double>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToDoubleTypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[double]",
        string? postfixSymbol = null,
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToDoubleTypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddFloatArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloFloatOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateFloatOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddFloatLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanFloatOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToFloatOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToFloatOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareFloatOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToFloatTypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[float]",
        string? postfixSymbol = "F")
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<float>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToFloatTypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[float]",
        string? postfixSymbol = "F",
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToFloatTypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddInt32ArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloInt32Operator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateInt32Operator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddInt32BitwiseOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseAndSymbol, new MathExpressionBitwiseAndInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseOrSymbol, new MathExpressionBitwiseOrInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseXorSymbol, new MathExpressionBitwiseXorInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseLeftShiftSymbol, new MathExpressionBitwiseLeftShiftInt32Operator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                new MathExpressionBitwiseRightShiftInt32Operator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.BitwiseNotSymbol, new MathExpressionBitwiseNotInt32Operator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseAndSymbol,
                MathExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseOrSymbol,
                MathExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseXorSymbol,
                MathExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                MathExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                MathExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.BitwiseNotSymbol,
                MathExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    public static MathExpressionFactoryBuilder AddInt32LogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanInt32Operator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToInt32Operator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToInt32Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareInt32Operator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToInt32TypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[int32]",
        string? postfixSymbol = null)
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<int>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToInt32TypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[int32]",
        string? postfixSymbol = null,
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToInt32TypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddInt64ArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloInt64Operator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateInt64Operator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddInt64BitwiseOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseAndSymbol, new MathExpressionBitwiseAndInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseOrSymbol, new MathExpressionBitwiseOrInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseXorSymbol, new MathExpressionBitwiseXorInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseLeftShiftSymbol, new MathExpressionBitwiseLeftShiftInt64Operator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                new MathExpressionBitwiseRightShiftInt64Operator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.BitwiseNotSymbol, new MathExpressionBitwiseNotInt64Operator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseAndSymbol,
                MathExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseOrSymbol,
                MathExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseXorSymbol,
                MathExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                MathExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                MathExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.BitwiseNotSymbol,
                MathExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    public static MathExpressionFactoryBuilder AddInt64LogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanInt64Operator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToInt64Operator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToInt64Operator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareInt64Operator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToInt64TypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[int64]",
        string? postfixSymbol = "L")
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<long>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToInt64TypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[int64]",
        string? postfixSymbol = "L",
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToInt64TypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddBigIntArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.SubtractSymbol, new MathExpressionSubtractBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.MultiplySymbol, new MathExpressionMultiplyBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.DivideSymbol, new MathExpressionDivideBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.ModuloSymbol, new MathExpressionModuloBigIntOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.NegateSymbol, new MathExpressionNegateBigIntOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.AddSymbol,
                MathExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.SubtractSymbol,
                MathExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.MultiplySymbol,
                MathExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.DivideSymbol,
                MathExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.ModuloSymbol,
                MathExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.NegateSymbol,
                MathExpressionConstructDefaults.NegatePrecedence );
    }

    public static MathExpressionFactoryBuilder AddBigIntBitwiseOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseAndSymbol, new MathExpressionBitwiseAndBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseOrSymbol, new MathExpressionBitwiseOrBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseXorSymbol, new MathExpressionBitwiseXorBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.BitwiseLeftShiftSymbol, new MathExpressionBitwiseLeftShiftBigIntOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                new MathExpressionBitwiseRightShiftBigIntOperator() )
            .AddPrefixUnaryOperator( MathExpressionConstructDefaults.BitwiseNotSymbol, new MathExpressionBitwiseNotBigIntOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseAndSymbol,
                MathExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseOrSymbol,
                MathExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseXorSymbol,
                MathExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                MathExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.BitwiseRightShiftSymbol,
                MathExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                MathExpressionConstructDefaults.BitwiseNotSymbol,
                MathExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    public static MathExpressionFactoryBuilder AddBigIntLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.GreaterThanSymbol, new MathExpressionGreaterThanBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.LessThanSymbol, new MathExpressionLessThanBigIntOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new MathExpressionGreaterThanOrEqualToBigIntOperator() )
            .AddBinaryOperator(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new MathExpressionLessThanOrEqualToBigIntOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareBigIntOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanSymbol,
                MathExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanSymbol,
                MathExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                MathExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.LessThanOrEqualToSymbol,
                MathExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToBigIntTypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[bigint]",
        string? postfixSymbol = null)
    {
        return AddTypeConverters( builder, new MathExpressionTypeConverter<BigInteger>(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToBigIntTypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[bigint]",
        string? postfixSymbol = null,
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToBigIntTypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddStringArithmeticOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.AddSymbol, new MathExpressionAddStringOperator() )
            .SetBinaryOperatorPrecedence( MathExpressionConstructDefaults.AddSymbol, MathExpressionConstructDefaults.AddPrecedence );
    }

    public static MathExpressionFactoryBuilder AddStringLogicalOperators(this MathExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( MathExpressionConstructDefaults.EqualToSymbol, new MathExpressionEqualToStringOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.NotEqualToSymbol, new MathExpressionNotEqualToStringOperator() )
            .AddBinaryOperator( MathExpressionConstructDefaults.CompareSymbol, new MathExpressionCompareStringOperator() )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.EqualToSymbol,
                MathExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.NotEqualToSymbol,
                MathExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                MathExpressionConstructDefaults.CompareSymbol,
                MathExpressionConstructDefaults.ComparePrecedence );
    }

    public static MathExpressionFactoryBuilder AddToStringTypeConversion(
        this MathExpressionFactoryBuilder builder,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol = "[string]",
        string? postfixSymbol = null)
    {
        return AddTypeConverters( builder, new MathExpressionToStringTypeConverter(), specializedConverters, symbol, postfixSymbol );
    }

    public static MathExpressionFactoryBuilder AddToStringTypeConversion(
        this MathExpressionFactoryBuilder builder,
        string symbol = "[string]",
        string? postfixSymbol = null,
        params MathExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddToStringTypeConversion( specializedConverters.AsEnumerable(), symbol, postfixSymbol );
    }

    private static MathExpressionFactoryBuilder AddTypeConverters(
        MathExpressionFactoryBuilder builder,
        MathExpressionTypeConverter genericConverter,
        IEnumerable<MathExpressionTypeConverter> specializedConverters,
        string symbol,
        string? postfixSymbol)
    {
        var specialized = specializedConverters.Materialize();

        builder
            .AddPrefixTypeConverter( symbol, genericConverter )
            .SetPrefixUnaryConstructPrecedence( symbol, MathExpressionConstructDefaults.TypeConverterPrecedence );

        foreach ( var specializedConverter in specialized )
            builder.AddPrefixTypeConverter( symbol, specializedConverter );

        if ( postfixSymbol is not null )
        {
            builder
                .AddPostfixTypeConverter( postfixSymbol, genericConverter )
                .SetPostfixUnaryConstructPrecedence( postfixSymbol, MathExpressionConstructDefaults.TypeConverterPrecedence );

            foreach ( var specializedConverter in specialized )
                builder.AddPostfixTypeConverter( postfixSymbol, specializedConverter );
        }

        return builder;
    }
}
