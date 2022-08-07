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
    PrefixUnaryOperatorDoesNotExist = 15,
    PostfixUnaryOperatorDoesNotExist = 16,
    PrefixTypeConverterDoesNotExist = 17,
    PostfixTypeConverterDoesNotExist = 18,
    BinaryOperatorDoesNotExist = 19,
    PrefixUnaryOperatorCollectionIsEmpty = 20,
    PrefixTypeConverterCollectionIsEmpty = 21,
    BinaryOperatorCollectionIsEmpty = 22,
    PostfixUnaryOrBinaryConstructDoesNotExist = 23,
    BinaryOrPrefixUnaryConstructDoesNotExist = 24,
    AmbiguousPostfixUnaryConstructResolutionFailure = 25
}
