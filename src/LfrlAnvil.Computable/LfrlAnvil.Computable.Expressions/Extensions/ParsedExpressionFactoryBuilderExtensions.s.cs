// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Constructs.BigInt;
using LfrlAnvil.Computable.Expressions.Constructs.Boolean;
using LfrlAnvil.Computable.Expressions.Constructs.Decimal;
using LfrlAnvil.Computable.Expressions.Constructs.Double;
using LfrlAnvil.Computable.Expressions.Constructs.Float;
using LfrlAnvil.Computable.Expressions.Constructs.Int32;
using LfrlAnvil.Computable.Expressions.Constructs.Int64;
using LfrlAnvil.Computable.Expressions.Constructs.String;
using LfrlAnvil.Computable.Expressions.Constructs.Variadic;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Extensions;

/// <summary>
/// Contains <see cref="ParsedExpressionFactoryBuilder"/> extension methods.
/// </summary>
public static class ParsedExpressionFactoryBuilderExtensions
{
    /// <summary>
    /// Adds default generic arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddGenericArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default generic bitwise operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary and (&amp;), binary or (|), binary xor (^), binary left shift (&lt;&lt;), binary right shift (&gt;&gt;)
    /// and prefix unary not (~).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddGenericBitwiseOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseAndSymbol, new ParsedExpressionBitwiseAndOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseOrSymbol, new ParsedExpressionBitwiseOrOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseXorSymbol, new ParsedExpressionBitwiseXorOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol, new ParsedExpressionBitwiseLeftShiftOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol, new ParsedExpressionBitwiseRightShiftOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.BitwiseNotSymbol, new ParsedExpressionBitwiseNotOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseAndSymbol,
                ParsedExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseOrSymbol,
                ParsedExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseXorSymbol,
                ParsedExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.BitwiseNotSymbol,
                ParsedExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    /// <summary>
    /// Adds default generic logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary null-coalesce (??), binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddGenericLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CoalesceSymbol, new ParsedExpressionCoalesceOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CoalesceSymbol,
                ParsedExpressionConstructDefaults.CoalescePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Boolean"/> bitwise operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>Operators include binary and (&amp;), binary or (|) and binary xor (^).</remarks>
    public static ParsedExpressionFactoryBuilder AddBooleanBitwiseOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseAndSymbol, new ParsedExpressionBitwiseAndBooleanOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseOrSymbol, new ParsedExpressionBitwiseOrBooleanOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseXorSymbol, new ParsedExpressionBitwiseXorBooleanOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseAndSymbol,
                ParsedExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseOrSymbol,
                ParsedExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseXorSymbol,
                ParsedExpressionConstructDefaults.BitwiseXorPrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Boolean"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary compare (&lt;=&gt;), binary logical and (and),
    /// binary logical or (or) and prefix unary logical not (not).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddBooleanLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToBooleanOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToBooleanOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareBooleanOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AndSymbol, new ParsedExpressionAndOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.OrSymbol, new ParsedExpressionOrOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NotSymbol, new ParsedExpressionNotOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AndSymbol,
                ParsedExpressionConstructDefaults.AndPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.OrSymbol,
                ParsedExpressionConstructDefaults.OrPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NotSymbol,
                ParsedExpressionConstructDefaults.NotPrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Boolean"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBooleanTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddBooleanTypeDefinition( ParsedExpressionConstructDefaults.BooleanTypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Boolean"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBooleanTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<bool>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Boolean"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBooleanTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddBooleanTypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Boolean"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBooleanTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddBooleanTypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Decimal"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddDecimalArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloDecimalOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateDecimalOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Decimal"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddDecimalLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanDecimalOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToDecimalOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToDecimalOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareDecimalOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Decimal"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDecimalTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddDecimalTypeDefinition( ParsedExpressionConstructDefaults.DecimalTypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Decimal"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDecimalTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<decimal>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Decimal"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDecimalTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddDecimalTypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Decimal"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDecimalTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddDecimalTypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Double"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddDoubleArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloDoubleOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateDoubleOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Double"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddDoubleLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanDoubleOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToDoubleOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToDoubleOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareDoubleOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Double"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDoubleTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddDoubleTypeDefinition( ParsedExpressionConstructDefaults.DoubleTypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Double"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDoubleTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<double>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Double"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDoubleTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddDoubleTypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Double"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDoubleTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddDoubleTypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Single"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddFloatArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloFloatOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateFloatOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Single"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddFloatLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanFloatOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToFloatOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToFloatOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareFloatOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Single"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddFloatTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddFloatTypeDefinition( ParsedExpressionConstructDefaults.FloatTypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Single"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddFloatTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<float>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Single"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddFloatTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddFloatTypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Single"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddFloatTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddFloatTypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddInt32ArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloInt32Operator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateInt32Operator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> bitwise operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary and (&amp;), binary or (|), binary xor (^), binary left shift (&lt;&lt;), binary right shift (&gt;&gt;)
    /// and prefix unary not (~).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddInt32BitwiseOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseAndSymbol, new ParsedExpressionBitwiseAndInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseOrSymbol, new ParsedExpressionBitwiseOrInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseXorSymbol, new ParsedExpressionBitwiseXorInt32Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                new ParsedExpressionBitwiseLeftShiftInt32Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                new ParsedExpressionBitwiseRightShiftInt32Operator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.BitwiseNotSymbol, new ParsedExpressionBitwiseNotInt32Operator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseAndSymbol,
                ParsedExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseOrSymbol,
                ParsedExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseXorSymbol,
                ParsedExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.BitwiseNotSymbol,
                ParsedExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddInt32LogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanInt32Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToInt32Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToInt32Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareInt32Operator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt32TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddInt32TypeDefinition( ParsedExpressionConstructDefaults.Int32TypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt32TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<int>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt32TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddInt32TypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Int32"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt32TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddInt32TypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddInt64ArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloInt64Operator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateInt64Operator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> bitwise operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary and (&amp;), binary or (|), binary xor (^), binary left shift (&lt;&lt;), binary right shift (&gt;&gt;)
    /// and prefix unary not (~).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddInt64BitwiseOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseAndSymbol, new ParsedExpressionBitwiseAndInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseOrSymbol, new ParsedExpressionBitwiseOrInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseXorSymbol, new ParsedExpressionBitwiseXorInt64Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                new ParsedExpressionBitwiseLeftShiftInt64Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                new ParsedExpressionBitwiseRightShiftInt64Operator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.BitwiseNotSymbol, new ParsedExpressionBitwiseNotInt64Operator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseAndSymbol,
                ParsedExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseOrSymbol,
                ParsedExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseXorSymbol,
                ParsedExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.BitwiseNotSymbol,
                ParsedExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddInt64LogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanInt64Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToInt64Operator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToInt64Operator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareInt64Operator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt64TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddInt64TypeDefinition( ParsedExpressionConstructDefaults.Int64TypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt64TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<long>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt64TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddInt64TypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="Int64"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddInt64TypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddInt64TypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary add (+), binary subtract (-), binary multiply (*), binary divide (/), binary modulo (mod)
    /// and prefix unary negate (-).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddBigIntArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.SubtractSymbol, new ParsedExpressionSubtractBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.MultiplySymbol, new ParsedExpressionMultiplyBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.DivideSymbol, new ParsedExpressionDivideBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.ModuloSymbol, new ParsedExpressionModuloBigIntOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.NegateSymbol, new ParsedExpressionNegateBigIntOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.AddSymbol,
                ParsedExpressionConstructDefaults.AddPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.SubtractSymbol,
                ParsedExpressionConstructDefaults.SubtractPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.MultiplySymbol,
                ParsedExpressionConstructDefaults.MultiplyPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.DivideSymbol,
                ParsedExpressionConstructDefaults.DividePrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.ModuloSymbol,
                ParsedExpressionConstructDefaults.ModuloPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.NegateSymbol,
                ParsedExpressionConstructDefaults.NegatePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> bitwise operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary and (&amp;), binary or (|), binary xor (^), binary left shift (&lt;&lt;), binary right shift (&gt;&gt;)
    /// and prefix unary not (~).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddBigIntBitwiseOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseAndSymbol, new ParsedExpressionBitwiseAndBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseOrSymbol, new ParsedExpressionBitwiseOrBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.BitwiseXorSymbol, new ParsedExpressionBitwiseXorBigIntOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                new ParsedExpressionBitwiseLeftShiftBigIntOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                new ParsedExpressionBitwiseRightShiftBigIntOperator() )
            .AddPrefixUnaryOperator( ParsedExpressionConstructDefaults.BitwiseNotSymbol, new ParsedExpressionBitwiseNotBigIntOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseAndSymbol,
                ParsedExpressionConstructDefaults.BitwiseAndPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseOrSymbol,
                ParsedExpressionConstructDefaults.BitwiseOrPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseXorSymbol,
                ParsedExpressionConstructDefaults.BitwiseXorPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseLeftShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseLeftShiftPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.BitwiseRightShiftSymbol,
                ParsedExpressionConstructDefaults.BitwiseRightShiftPrecedence )
            .SetPrefixUnaryConstructPrecedence(
                ParsedExpressionConstructDefaults.BitwiseNotSymbol,
                ParsedExpressionConstructDefaults.BitwiseNotPrecedence );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=), binary greater than (&gt;),
    /// binary less than (&lt;), binary greater than or equal to (&gt;=), binary less than or equal to (&lt;=)
    /// and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddBigIntLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.GreaterThanSymbol, new ParsedExpressionGreaterThanBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.LessThanSymbol, new ParsedExpressionLessThanBigIntOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                new ParsedExpressionGreaterThanOrEqualToBigIntOperator() )
            .AddBinaryOperator(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                new ParsedExpressionLessThanOrEqualToBigIntOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareBigIntOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanSymbol,
                ParsedExpressionConstructDefaults.GreaterThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanSymbol,
                ParsedExpressionConstructDefaults.LessThanPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.GreaterThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.LessThanOrEqualToSymbol,
                ParsedExpressionConstructDefaults.LessThanOrEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBigIntTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddBigIntTypeDefinition( ParsedExpressionConstructDefaults.BigIntTypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBigIntTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionTypeConverter<BigInteger>(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBigIntTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddBigIntTypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="BigInteger"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddBigIntTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddBigIntTypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="String"/> arithmetic operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>Operators include binary add (+).</remarks>
    public static ParsedExpressionFactoryBuilder AddStringArithmeticOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.AddSymbol, new ParsedExpressionAddStringOperator() )
            .SetBinaryOperatorPrecedence( ParsedExpressionConstructDefaults.AddSymbol, ParsedExpressionConstructDefaults.AddPrecedence );
    }

    /// <summary>
    /// Adds default <see cref="String"/> logical operator constructs with precedences to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>
    /// Operators include binary equal to (==), binary not equal to (!=) and binary compare (&lt;=&gt;).
    /// </remarks>
    public static ParsedExpressionFactoryBuilder AddStringLogicalOperators(this ParsedExpressionFactoryBuilder builder)
    {
        return builder
            .AddBinaryOperator( ParsedExpressionConstructDefaults.EqualToSymbol, new ParsedExpressionEqualToStringOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.NotEqualToSymbol, new ParsedExpressionNotEqualToStringOperator() )
            .AddBinaryOperator( ParsedExpressionConstructDefaults.CompareSymbol, new ParsedExpressionCompareStringOperator() )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.EqualToSymbol,
                ParsedExpressionConstructDefaults.EqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.NotEqualToSymbol,
                ParsedExpressionConstructDefaults.NotEqualToPrecedence )
            .SetBinaryOperatorPrecedence(
                ParsedExpressionConstructDefaults.CompareSymbol,
                ParsedExpressionConstructDefaults.ComparePrecedence );
    }

    /// <summary>
    /// Adds default <see cref="String"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddStringTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return builder.AddStringTypeDefinition( ParsedExpressionConstructDefaults.StringTypeSymbols, specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="String"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddStringTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        return AddTypeDefinition( builder, symbols, new ParsedExpressionToStringTypeConverter(), specializedConverters );
    }

    /// <summary>
    /// Adds default <see cref="String"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddStringTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddStringTypeDefinition( specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default <see cref="String"/> type definition to the provided <paramref name="builder"/>.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Symbols to use.</param>
    /// <param name="specializedConverters">Collection of specialized type converters.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddStringTypeDefinition(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        params ParsedExpressionTypeConverter[] specializedConverters)
    {
        return builder.AddStringTypeDefinition( symbols, specializedConverters.AsEnumerable() );
    }

    /// <summary>
    /// Adds default branching variadic function constructs.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="symbols">Optional custom symbols.</param>
    /// <returns><paramref name="builder"/>.</returns>
    /// <remarks>Constructs include <b>if</b>, <b>switch-case</b>, <b>switch</b> and <b>throw</b> functions.</remarks>
    public static ParsedExpressionFactoryBuilder AddBranchingVariadicFunctions(
        this ParsedExpressionFactoryBuilder builder,
        ParsedExpressionBranchingVariadicFunctionSymbols symbols = default)
    {
        return builder
            .AddVariadicFunction( symbols.If, new ParsedExpressionIf() )
            .AddVariadicFunction( symbols.SwitchCase, new ParsedExpressionSwitchCase() )
            .AddVariadicFunction( symbols.Switch, new ParsedExpressionSwitch() )
            .AddVariadicFunction( symbols.Throw, new ParsedExpressionThrow() );
    }

    /// <summary>
    /// Adds missing default unary construct precedences.
    /// </summary>
    /// <param name="builder">Source builder.</param>
    /// <param name="defaultPrecedence">Default unary construct precedence. Equal to <b>1</b> by default.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static ParsedExpressionFactoryBuilder AddDefaultUnaryConstructPrecedences(
        this ParsedExpressionFactoryBuilder builder,
        int defaultPrecedence = ParsedExpressionConstructDefaults.DefaultUnaryPrecedence)
    {
        var unaryConstructs = builder.GetConstructs()
            .Where( static i => (i.Type & ParsedExpressionConstructType.UnaryConstruct) != ParsedExpressionConstructType.None );

        var prefixPrecedences = builder.GetPrefixUnaryConstructPrecedences()
            .Select( static kv => kv.Key )
            .ToHashSet();

        var postfixPrecedences = builder.GetPostfixUnaryConstructPrecedences()
            .Select( static kv => kv.Key )
            .ToHashSet();

        foreach ( var info in unaryConstructs )
        {
            if ( (info.Type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None )
            {
                if ( ! prefixPrecedences.Contains( info.Symbol ) )
                    builder.SetPrefixUnaryConstructPrecedence( info.Symbol, defaultPrecedence );

                continue;
            }

            if ( ! postfixPrecedences.Contains( info.Symbol ) )
                builder.SetPostfixUnaryConstructPrecedence( info.Symbol, defaultPrecedence );
        }

        return builder;
    }

    private static ParsedExpressionFactoryBuilder AddTypeDefinition(
        ParsedExpressionFactoryBuilder builder,
        ParsedExpressionTypeDefinitionSymbols symbols,
        ParsedExpressionTypeConverter genericConverter,
        IEnumerable<ParsedExpressionTypeConverter> specializedConverters)
    {
        var specialized = specializedConverters.Materialize();
        builder.AddTypeDeclaration( symbols.Name, genericConverter.TargetType );

        var prefixSymbol = symbols.PrefixTypeConverter;
        var postfixSymbol = symbols.PostfixTypeConverter;
        var constantSymbol = symbols.Constant;

        if ( prefixSymbol is not null )
        {
            builder
                .AddPrefixTypeConverter( prefixSymbol.Value, genericConverter )
                .SetPrefixUnaryConstructPrecedence( prefixSymbol.Value, ParsedExpressionConstructDefaults.TypeConverterPrecedence );

            foreach ( var specializedConverter in specialized )
                builder.AddPrefixTypeConverter( prefixSymbol.Value, specializedConverter );
        }

        if ( postfixSymbol is not null )
        {
            builder
                .AddPostfixTypeConverter( postfixSymbol.Value, genericConverter )
                .SetPostfixUnaryConstructPrecedence( postfixSymbol.Value, ParsedExpressionConstructDefaults.TypeConverterPrecedence );

            foreach ( var specializedConverter in specialized )
                builder.AddPostfixTypeConverter( postfixSymbol.Value, specializedConverter );
        }

        if ( constantSymbol is not null )
            builder.AddConstant( constantSymbol.Value, new ParsedExpressionConstant<Type>( genericConverter.TargetType ) );

        return builder;
    }
}
