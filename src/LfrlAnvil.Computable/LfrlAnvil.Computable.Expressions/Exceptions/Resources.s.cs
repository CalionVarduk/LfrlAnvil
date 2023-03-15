using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using LfrlAnvil.Computable.Expressions.Constructs;
using LfrlAnvil.Computable.Expressions.Errors;
using LfrlAnvil.Computable.Expressions.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Exceptions;

internal static class Resources
{
    internal const string CannotBindValueToArgumentThatDoesNotExist = "Cannot bind a value to an argument that doesn't exist.";
    internal const string InvocationHasThrownAnException = "Invocation has thrown an exception.";

    internal const string CannotDetermineIfReturnType =
        "Cannot determine IF return type due to both TRUE and FALSE bodies representing throw expressions.";

    internal const string CannotDetermineSwitchReturnType =
        "Cannot determine SWITCH return type due to all CASE bodies representing throw expressions.";

    internal const string SwitchValueWasNotHandledByAnyCaseFormat = "SWITCH value '{0}' was not handled by any CASE.";
    internal const string MemberNameMustBeConstantNonNullString = "Member name must be a constant non-null string.";
    internal const string ArrayElementTypeMustBeConstantNonNullType = "Array element type must be a constant non-null type.";
    internal const string CtorTypeMustBeConstantNonNullType = "Constructed type must be a constant non-null type.";

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
            $"Found duplicate binary operator for symbol '{symbol}' (left argument type: {@operator.LeftArgumentType.GetDebugString()}, right argument type: {@operator.RightArgumentType.GetDebugString()}).";
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

        return
            $"Found duplicate {typeText} unary operator for symbol '{symbol}' (argument type: {@operator.ArgumentType.GetDebugString()}).";
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

        Assume.IsNotNull( converter.SourceType, nameof( converter.SourceType ) );
        return $"Found duplicate {typeText} type converter for symbol '{symbol}' (source type: {converter.SourceType.GetDebugString()}).";
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

        return $"Not all {typeText} type converters for symbol '{symbol}' have the same {targetType.GetDebugString()} target type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string TypeConverterCollectionsDoNotHaveTheSameTargetType(
        StringSlice symbol,
        Type prefixTargetType,
        Type postfixTargetType)
    {
        return
            $"Type converter collections for symbol '{symbol}' don't have the same target type (prefix: {prefixTargetType.GetDebugString()}, postfix: {postfixTargetType.GetDebugString()}).";
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
        var parameterTypesText = string.Join( ", ", parameters.Select( static e => e.Type.GetDebugString() ) );
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
        return $"Expected '{paramName}' to contain {expected} element(s) but found {actual}.";
    }

    [Pure]
    internal static string InvalidExpressionArguments(Chain<StringSlice> argumentNames)
    {
        var headerText = $"Expression doesn't contain following arguments:{Environment.NewLine}";
        var allArgumentsText = string.Join( Environment.NewLine, argumentNames.Select( static (n, i) => $"{i + 1}. {n}" ) );
        return $"{headerText}{allArgumentsText}";
    }

    [Pure]
    internal static string FailedExpressionCreation(string input, Chain<ParsedExpressionBuilderError> errors)
    {
        var headerText = $"Failed to create an expression:{Environment.NewLine}{input}{Environment.NewLine}{Environment.NewLine}";
        var errorsHeaderText = $"Encountered {errors.Count} error(s):{Environment.NewLine}";
        var allErrorsText = string.Join( Environment.NewLine, errors.Select( static (e, i) => $"{i + 1}. {e}" ) );
        return $"{headerText}{errorsHeaderText}{allErrorsText}";
    }

    [Pure]
    internal static string FailedExpressionFactoryCreation(Chain<string> messages)
    {
        var headerText = $"Builder has encountered {messages.Count} error(s):";
        var allMessagesText = string.Join( Environment.NewLine, messages.Select( static (m, i) => $"{i + 1}. {m}" ) );
        return $"{headerText}{Environment.NewLine}{allMessagesText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidTypeConverterResultType(Type actual, Type expected)
    {
        return
            $"Expected type converter to return result of type assignable to {expected.GetDebugString()} but found {actual.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string ConstructFailedToFindCompareToMethod(Type targetType, Type parameterType, Type constructType)
    {
        var methodSignature =
            $"{typeof( int ).GetDebugString()} {targetType.GetDebugString()}.{nameof( IComparable.CompareTo )}({parameterType.GetDebugString()})";

        return $"{constructType.GetDebugString()} has failed to find method {methodSignature}.";
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
        return $"Expected IF test type to be {typeof( bool ).GetDebugString()} but found {testType.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidSwitchCaseParameter(int index)
    {
        return $"Expected SWITCH parameter at index {index} to be a {nameof( SwitchCase )}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnresolvableMember(
        Type targetType,
        MemberTypes memberType,
        string memberName,
        IReadOnlyList<Type>? parameterTypes)
    {
        var distinctMemberTypes = Enumerable.Range( 0, 8 )
            .Where( i => (((int)memberType >> i) & 1) == 1 )
            .Select( static i => (MemberTypes)(1 << i) );

        var memberTypeText = string.Join( " or ", distinctMemberTypes );

        var parametersText = parameterTypes is null
            ? string.Empty
            : $" (parameter types: [{string.Join( ", ", parameterTypes.Select( static p => p.GetDebugString() ) )}])";

        return $"{memberTypeText} member '{memberName}'{parametersText} could not be resolved for {targetType.GetDebugString()} type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnresolvableIndexer(Type targetType, IReadOnlyList<Type> parameterTypes)
    {
        var parametersText = $"(parameter types: [{string.Join( ", ", parameterTypes.Select( static p => p.GetDebugString() ) )}])";
        return $"Indexer member {parametersText} could not be resolved for {targetType.GetDebugString()} type.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string AmbiguousMembers(Type targetType, string memberName, IReadOnlyList<MemberInfo> members)
    {
        var headerText = $"Found {members.Count} ambiguous '{memberName}' members in {targetType.GetDebugString()} type:";
        var membersText = string.Join(
            Environment.NewLine,
            members.Select(
                static (member, i) =>
                {
                    var memberText = member switch
                    {
                        FieldInfo f => f.GetDebugString(),
                        PropertyInfo p => p.GetDebugString(),
                        MethodInfo m => m.GetDebugString(),
                        _ => member.ToString() ?? string.Empty
                    };

                    return $"{i + 1}. {memberText}";
                } ) );

        return $"{headerText}{Environment.NewLine}{membersText}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string InvalidArrayElementType(Type expectedType, Type actualType)
    {
        return
            $"Expected all elements in an array to be assignable to {expectedType.GetDebugString()} type but found {actualType.GetDebugString()}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string UnsupportedDelegateParameterCount(int parameterCount)
    {
        return $"Inline delegates with captured parameters cannot support more than 15 parameters but found {parameterCount}.";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string NonInvocableType(Type type)
    {
        return $"Type {type.GetDebugString()} is not invocable.";
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
