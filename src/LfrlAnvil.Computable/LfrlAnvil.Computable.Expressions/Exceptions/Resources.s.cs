using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

internal static class Resources
{
    internal const string CannotBindValueToArgumentThatDoesNotExist = "Cannot bind a value to an argument that doesn't exist.";
    internal const string InvocationHasThrownAnException = "Invocation has thrown an exception.";

    internal const string CannotDetermineIfReturnType =
        "Cannot determine IF return type due to both TRUE and FALSE bodies representing throw expressions.";

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
            nameof( IParsedExpressionFactoryConfiguration.DecimalPoint ),
            nameof( IParsedExpressionFactoryConfiguration.IntegerDigitSeparator ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DecimalPointAndStringDelimiterMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IParsedExpressionFactoryConfiguration.DecimalPoint ),
            nameof( IParsedExpressionFactoryConfiguration.StringDelimiter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string DecimalPointAndScientificNotationExponentsMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IParsedExpressionFactoryConfiguration.DecimalPoint ),
            nameof( IParsedExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string IntegerDigitSeparatorAndStringDelimiterMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IParsedExpressionFactoryConfiguration.IntegerDigitSeparator ),
            nameof( IParsedExpressionFactoryConfiguration.StringDelimiter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string IntegerDigitSeparatorAndScientificNotationExponentsMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IParsedExpressionFactoryConfiguration.IntegerDigitSeparator ),
            nameof( IParsedExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string StringDelimiterAndScientificNotationExponentsMustBeDifferent()
    {
        return ConfigurationSymbolsMustBeDifferent(
            nameof( IParsedExpressionFactoryConfiguration.StringDelimiter ),
            nameof( IParsedExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidDecimalPointSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IParsedExpressionFactoryConfiguration.DecimalPoint ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidIntegerDigitSeparatorSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IParsedExpressionFactoryConfiguration.IntegerDigitSeparator ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidStringDelimiterSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IParsedExpressionFactoryConfiguration.StringDelimiter ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidScientificNotationExponentsSymbol(char symbol)
    {
        return InvalidConfigurationSymbol( symbol, nameof( IParsedExpressionFactoryConfiguration.ScientificNotationExponents ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AtLeastOneScientificNotationExponentSymbolMustBeDefined()
    {
        return $"At least one {nameof( IParsedExpressionFactoryConfiguration.ScientificNotationExponents )} symbol must be defined.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string OperatorGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return $"Expected construct group with '{symbol}' symbol to be comprised of only operators but found other construct types.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateTypedBinaryOperator(StringSlice symbol, ParsedExpressionTypedBinaryOperator @operator)
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
        ParsedExpressionTypedUnaryOperator @operator,
        ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "prefix"
            : "postfix";

        return $"Found duplicate {typeText} unary operator for symbol '{symbol}' (argument type: {@operator.ArgumentType.FullName}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateGenericUnaryOperator(StringSlice symbol, ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "prefix"
            : "postfix";

        return $"Found duplicate generic {typeText} unary operator for symbol '{symbol}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UndefinedUnaryOperatorPrecedence(StringSlice symbol, ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "Prefix"
            : "Postfix";

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
        ParsedExpressionTypeConverter converter,
        ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "prefix"
            : "postfix";

        return $"Found duplicate {typeText} type converter for symbol '{symbol}' (source type: {converter.SourceType!.FullName}).";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateGenericTypeConverter(StringSlice symbol, ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "prefix"
            : "postfix";

        return $"Found duplicate generic {typeText} type converter for symbol '{symbol}'.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UndefinedTypeConverterPrecedence(StringSlice symbol, ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "Prefix"
            : "Postfix";

        return $"{typeText} type converter precedence for symbol '{symbol}' is undefined.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NotAllTypeConvertersHaveTheSameTargetType(
        StringSlice symbol,
        Type targetType,
        ParsedExpressionConstructType type)
    {
        var typeText = (type & ParsedExpressionConstructType.PrefixUnaryConstruct) != ParsedExpressionConstructType.None
            ? "prefix"
            : "postfix";

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
    internal static string ConstantGroupContainsMoreThanOneConstant(StringSlice symbol)
    {
        return $"Expected constant group with '{symbol}' symbol to contain only one constant.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ConstantGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return $"Expected constant group with '{symbol}' symbol to be comprised of only constants but found other construct types.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TypeDeclarationGroupContainsMoreThanOneType(StringSlice symbol)
    {
        return $"Expected type declaration group with '{symbol}' symbol to contain only one type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TypeDeclarationGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return $"Expected type declaration group with '{symbol}' symbol to be comprised of only types but found other construct types.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string VariadicFunctionGroupContainsMoreThanOneFunction(StringSlice symbol)
    {
        return $"Expected variadic function group with '{symbol}' symbol to contain only one function.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string VariadicFunctionGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return
            $"Expected variadic function group with '{symbol}' symbol to be comprised of only variadic functions but found other construct types.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FoundDuplicateFunctionSignature(
        StringSlice symbol,
        IReadOnlyList<Expression> parameters)
    {
        var parameterTypesText = string.Join( ", ", parameters.Select( e => e.Type.FullName ) );
        return $"Found duplicate function signature for symbol '{symbol}' (parameter types: [{parameterTypesText}])";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string FunctionGroupContainsConstructsOfOtherType(StringSlice symbol)
    {
        return $"Expected function group with '{symbol}' symbol to be comprised of only functions but found other construct types.";
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
        var allArgumentsText = string.Join( Environment.NewLine, argumentNames.Select( (n, i) => $"{i + 1}. {n}" ) );
        return $"{headerText}{allArgumentsText}";
    }

    [Pure]
    internal static string FailedExpressionCreation(string input, Chain<ParsedExpressionBuilderError> errors)
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
    internal static string IfTestMustBeOfBooleanType(Type testType)
    {
        return $"Expected IF test type to be {typeof( bool ).FullName} but found {testType.FullName}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidSwitchCaseParameter(int index)
    {
        return $"Expected SWITCH parameter at index {index} to be a {nameof( SwitchCase )}.";
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
