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

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents an <see cref="IParsedExpressionFactory"/> configuration.
/// </summary>
public interface IParsedExpressionFactoryConfiguration
{
    /// <summary>
    /// Specifies the decimal point.
    /// </summary>
    char DecimalPoint { get; }

    /// <summary>
    /// Specifies the integer digit separator.
    /// </summary>
    char IntegerDigitSeparator { get; }

    /// <summary>
    /// Specifies characters that represent scientific notation exponents.
    /// </summary>
    string ScientificNotationExponents { get; }

    /// <summary>
    /// Specifies whether or not non-integer numbers are allowed.
    /// </summary>
    bool AllowNonIntegerNumbers { get; }

    /// <summary>
    /// Specifies whether or not scientific notation for numbers is allowed.
    /// </summary>
    bool AllowScientificNotation { get; }

    /// <summary>
    /// Specifies constant string delimiter symbol.
    /// </summary>
    char StringDelimiter { get; }

    /// <summary>
    /// Specifies whether or not the expression's result is automatically converted to the expected result type.
    /// </summary>
    bool ConvertResultToOutputTypeAutomatically { get; }

    /// <summary>
    /// Specifies whether or not non-public member access is allowed.
    /// </summary>
    bool AllowNonPublicMemberAccess { get; }

    /// <summary>
    /// Specifies whether or not member names are case insensitive.
    /// </summary>
    bool IgnoreMemberNameCase { get; }

    /// <summary>
    /// Specifies whether or not nested static delegate expressions should not be compiled immediately.
    /// </summary>
    bool PostponeStaticInlineDelegateCompilation { get; }

    /// <summary>
    /// Specifies whether or not unused arguments should be discarded.
    /// </summary>
    bool DiscardUnusedArguments { get; }
}
