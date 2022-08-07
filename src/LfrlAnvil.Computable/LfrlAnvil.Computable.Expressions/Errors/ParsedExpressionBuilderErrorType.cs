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
    ConstructConsumedInvalidAmountOfOperands = 9,
    ExpressionMustContainAtLeastOneOperand = 10,
    ExpressionContainsInvalidOperandToOperatorRatio = 11,
    ExpressionContainsUnclosedParentheses = 12,
    UnclosedParenthesis = 13,
    OutputTypeConverterHasThrownException = 14,
    ExpressionResultTypeIsNotCompatibleWithExpectedOutputType = 15,
    PrefixUnaryOperatorDoesNotExist = 16,
    PostfixUnaryOperatorDoesNotExist = 17,
    PrefixTypeConverterDoesNotExist = 18,
    PostfixTypeConverterDoesNotExist = 19,
    BinaryOperatorDoesNotExist = 20,
    PrefixUnaryOperatorCollectionIsEmpty = 21,
    PrefixTypeConverterCollectionIsEmpty = 22,
    BinaryOperatorCollectionIsEmpty = 23,
    PostfixUnaryOrBinaryConstructDoesNotExist = 24,
    BinaryOrPrefixUnaryConstructDoesNotExist = 25,
    AmbiguousPostfixUnaryConstructResolutionFailure = 26
}
