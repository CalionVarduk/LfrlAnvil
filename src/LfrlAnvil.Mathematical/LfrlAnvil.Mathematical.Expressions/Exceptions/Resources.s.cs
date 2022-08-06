﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Mathematical.Expressions.Constructs;
using LfrlAnvil.Mathematical.Expressions.Errors;
using LfrlAnvil.Mathematical.Expressions.Internal;

namespace LfrlAnvil.Mathematical.Expressions.Exceptions;

internal static class Resources
{
    internal const string CannotBindValueToArgumentThatDoesNotExist = "Cannot bind a value to an argument that doesn't exist.";

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidConstructSymbol(StringSlice symbol)
    {
        return $"'{symbol}' is not a valid construct symbol.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DecimalPointAndIntegerDigitSeparatorMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IMathExpressionFactoryConfiguration.DecimalPoint ),
            nameof( IMathExpressionFactoryConfiguration.IntegerDigitSeparator ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DecimalPointAndStringDelimiterMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IMathExpressionFactoryConfiguration.DecimalPoint ),
            nameof( IMathExpressionFactoryConfiguration.StringDelimiter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DecimalPointAndScientificNotationExponentsMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IMathExpressionFactoryConfiguration.DecimalPoint ),
            nameof( IMathExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string IntegerDigitSeparatorAndStringDelimiterMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IMathExpressionFactoryConfiguration.IntegerDigitSeparator ),
            nameof( IMathExpressionFactoryConfiguration.StringDelimiter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string IntegerDigitSeparatorAndScientificNotationExponentsMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IMathExpressionFactoryConfiguration.IntegerDigitSeparator ),
            nameof( IMathExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringDelimiterAndScientificNotationExponentsMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IMathExpressionFactoryConfiguration.StringDelimiter ),
            nameof( IMathExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDecimalPointSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IMathExpressionFactoryConfiguration.DecimalPoint ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidIntegerDigitSeparatorSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IMathExpressionFactoryConfiguration.IntegerDigitSeparator ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidStringDelimiterSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IMathExpressionFactoryConfiguration.StringDelimiter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidScientificNotationExponentsSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IMathExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AtLeastOneScientificNotationExponentSymbolMustBeDefined()
    {
        return $"At least one {nameof( IMathExpressionFactoryConfiguration.ScientificNotationExponents )} symbol must be defined.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string OperatorGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return $"Expected construct group with '{symbol}' symbol to be comprised of only operators but found other construct types.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateTypedBinaryOperator(StringSlice symbol, MathExpressionTypedBinaryOperator @operator)
    {
        return
            $"Found duplicate binary operator for symbol '{symbol}' (left argument type: {@operator.LeftArgumentType.FullName}, right argument type: {@operator.RightArgumentType.FullName}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateGenericBinaryOperator(StringSlice symbol)
    {
        return $"Found duplicate generic binary operator for symbol '{symbol}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UndefinedBinaryOperatorPrecedence(StringSlice symbol)
    {
        return $"Binary operator precedence for symbol '{symbol}' is undefined.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateTypedUnaryOperator(
        StringSlice symbol,
        MathExpressionTypedUnaryOperator @operator,
        MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "prefix" : "postfix";
        return $"Found duplicate {typeText} unary operator for symbol '{symbol}' (argument type: {@operator.ArgumentType.FullName}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateGenericUnaryOperator(StringSlice symbol, MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "prefix" : "postfix";
        return $"Found duplicate generic {typeText} unary operator for symbol '{symbol}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UndefinedUnaryOperatorPrecedence(StringSlice symbol, MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "Prefix" : "Postfix";
        return $"{typeText} unary operator precedence for symbol '{symbol}' is undefined.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TypeConverterGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return $"Expected construct group with '{symbol}' symbol to be comprised of only type converters but found other construct types.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateTypedTypeConverter(
        StringSlice symbol,
        MathExpressionTypeConverter converter,
        MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "prefix" : "postfix";
        return $"Found duplicate {typeText} type converter for symbol '{symbol}' (source type: {converter.SourceType!.FullName}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateGenericTypeConverter(StringSlice symbol, MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "prefix" : "postfix";
        return $"Found duplicate generic {typeText} type converter for symbol '{symbol}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UndefinedTypeConverterPrecedence(StringSlice symbol, MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "Prefix" : "Postfix";
        return $"{typeText} type converter precedence for symbol '{symbol}' is undefined.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotAllTypeConvertersHaveTheSameTargetType(
        StringSlice symbol,
        Type targetType,
        MathExpressionFactoryBuilder.ConstructType type)
    {
        var typeText = type == MathExpressionFactoryBuilder.ConstructType.PrefixUnaryConstruct ? "prefix" : "postfix";
        return $"Not all {typeText} type converters for symbol '{symbol}' have the same {targetType.FullName} target type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TypeConverterCollectionsDoNotHaveTheSameTargetType(
        StringSlice symbol,
        Type prefixTargetType,
        Type postfixTargetType)
    {
        return
            $"Type converter collections for symbol '{symbol}' don't have the same target type (prefix: {prefixTargetType.FullName}, postfix: {postfixTargetType.FullName}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidExpressionArgumentCount(int actual, int expected, string paramName)
    {
        return $"Expected '{paramName}' to contain {expected} elements but found {actual}.";
    }

    [Pure]
    internal static string InvalidExpressionArguments(Chain<ReadOnlyMemory<char>> argumentNames)
    {
        var headerText = $"Expression doesn't contain following arguments:{Environment.NewLine}";
        var allArgumentsText = argumentNames.Select( (n, i) => $"{i + 1}. {n}" );
        return $"{headerText}{allArgumentsText}";
    }

    [Pure]
    internal static string FailedExpressionCreation(string input, Chain<MathExpressionBuilderError> errors)
    {
        var headerText = $"Failed to create an expression:{Environment.NewLine}{input}{Environment.NewLine}{Environment.NewLine}";
        var errorsHeaderText = $"Encountered {errors.Count} error(s):{Environment.NewLine}";
        var allErrorsText = string.Join( Environment.NewLine, errors.Select( (e, i) => $"{i + 1}. {e}" ) );
        return $"{headerText}{errorsHeaderText}{allErrorsText}";
    }

    [Pure]
    internal static string FailedExpressionFactoryCreation(Chain<string> messages)
    {
        var headerText = $"Builder has encountered {messages.Count} error(s):";
        var allMessagesText = string.Join( Environment.NewLine, messages.Select( (m, i) => $"{i + 1}. {m}" ) );
        return $"{headerText}{Environment.NewLine}{allMessagesText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidTypeConverterResultType(Type actual, Type expected)
    {
        return $"Expected type converter to return result of type assignable to {expected.FullName} but found {actual.FullName}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ConstructFailedToFindCompareToMethod(Type targetType, Type parameterType, Type constructType)
    {
        var methodSignature = $"{typeof( int ).FullName} {targetType}.{nameof( IComparable.CompareTo )}({parameterType})";
        return $"{constructType.FullName} has failed to find method {methodSignature}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ArgumentBufferIsTooSmall(int actualLength, int expectedMinLength)
    {
        return $"Expected argument buffer's length to be greater than or equal to {expectedMinLength} but found {actualLength}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string ConfigurationSymbolsMustBeDifferent(string firstSymbolName, string secondSymbolName)
    {
        return $"{firstSymbolName} and {secondSymbolName} symbols must be different.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string InvalidConfigurationSymbol(char symbol, string symbolName)
    {
        return $"'{symbol}' is not a valid {symbolName} symbol.";
    }
}
