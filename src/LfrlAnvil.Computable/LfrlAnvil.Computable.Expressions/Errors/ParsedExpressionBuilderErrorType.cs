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

namespace LfrlAnvil.Computable.Expressions.Errors;

/// <summary>
/// Represents a type of an error that occurred during <see cref="IParsedExpression{TArg,TResult}"/> creation.
/// </summary>
public enum ParsedExpressionBuilderErrorType : byte
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    Error = 0,
    UnexpectedOperand = 1,
    UnexpectedDelegateParameterName = 2,
    UnexpectedFunctionCall = 3,
    UnexpectedTypeDeclaration = 4,
    UnexpectedConstruct = 5,
    UnexpectedElementSeparator = 6,
    UnexpectedOpenedParenthesis = 7,
    UnexpectedClosedParenthesis = 8,
    UnexpectedOpenedSquareBracket = 9,
    UnexpectedClosedSquareBracket = 10,
    UnexpectedMemberAccess = 11,
    UnexpectedLocalTermDeclaration = 12,
    UnexpectedAssignment = 13,
    UnexpectedLineSeparator = 14,
    UnexpectedEnd = 15,
    UndeclaredLocalTermUsage = 16,
    NumberConstantParsingFailure = 17,
    StringConstantParsingFailure = 18,
    InvalidArgumentName = 19,
    InvalidDelegateParameterName = 20,
    InvalidMacroParameterName = 21,
    InvalidLocalTermName = 22,
    DuplicatedDelegateParameterName = 23,
    DuplicatedMacroParameterName = 24,
    DuplicatedLocalTermName = 25,
    ConstructHasThrownException = 26,
    MacroMustContainAtLeastOneToken = 27,
    MacroParameterMustContainAtLeastOneToken = 28,
    ExpressionMustContainAtLeastOneOperand = 29,
    ExpressionContainsInvalidOperandToOperatorRatio = 30,
    MissingSubExpressionClosingSymbol = 31,
    ExpressionContainsUnclosedParentheses = 32,
    UnclosedParenthesis = 33,
    OutputTypeConverterHasThrownException = 34,
    ExpressionResultTypeIsNotCompatibleWithExpectedOutputType = 35,
    PrefixUnaryOperatorCouldNotBeResolved = 36,
    PostfixUnaryOperatorCouldNotBeResolved = 37,
    PrefixTypeConverterCouldNotBeResolved = 38,
    PostfixTypeConverterCouldNotBeResolved = 39,
    BinaryOperatorCouldNotBeResolved = 40,
    FunctionCouldNotBeResolved = 41,
    InvalidMacroParameterCount = 42,
    ExpectedPrefixUnaryConstruct = 43,
    ExpectedBinaryOperator = 44,
    ExpectedPostfixUnaryOrBinaryConstruct = 45,
    ExpectedBinaryOrPrefixUnaryConstruct = 46,
    AmbiguousPostfixUnaryConstructResolutionFailure = 47,
    NestedExpressionFailure = 48,
    MacroResolutionFailure = 49,
    MacroParameterResolutionFailure = 50,
    InlineDelegateError = 51,
    LocalTermError = 52
}
