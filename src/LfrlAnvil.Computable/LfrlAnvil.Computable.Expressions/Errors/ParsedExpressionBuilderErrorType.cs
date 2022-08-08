namespace LfrlAnvil.Computable.Expressions.Errors;

public enum ParsedExpressionBuilderErrorType : byte
{
    Error = 0,
    UnexpectedOperand = 1,
    UnexpectedConstruct = 2,
    NumberConstantParsingFailure = 3,
    StringConstantParsingFailure = 4,
    InvalidArgumentName = 5,
    UnexpectedOpenedParenthesis = 6,
    UnexpectedClosedParenthesis = 7,
    ConstructHasThrownException = 8,
    ExpressionMustContainAtLeastOneOperand = 9,
    ExpressionContainsInvalidOperandToOperatorRatio = 10,
    ExpressionContainsUnclosedParentheses = 11,
    UnclosedParenthesis = 12,
    OutputTypeConverterHasThrownException = 13,
    ExpressionResultTypeIsNotCompatibleWithExpectedOutputType = 14,
    PrefixUnaryOperatorCouldNotBeResolved = 15,
    PostfixUnaryOperatorCouldNotBeResolved = 16,
    PrefixTypeConverterCouldNotBeResolved = 17,
    PostfixTypeConverterCouldNotBeResolved = 18,
    BinaryOperatorCouldNotBeResolved = 19,
    ExpectedPrefixUnaryConstruct = 20,
    ExpectedBinaryOperator = 21,
    ExpectedPostfixUnaryOrBinaryConstruct = 22,
    ExpectedBinaryOrPrefixUnaryConstruct = 23,
    AmbiguousPostfixUnaryConstructResolutionFailure = 24
}
