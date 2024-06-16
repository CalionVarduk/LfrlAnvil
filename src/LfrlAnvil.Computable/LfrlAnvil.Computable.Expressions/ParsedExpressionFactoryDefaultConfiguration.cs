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
/// Represents an <see cref="IParsedExpressionFactory"/> configuration with default values.
/// </summary>
public class ParsedExpressionFactoryDefaultConfiguration : IParsedExpressionFactoryConfiguration
{
    /// <inheritdoc />
    public virtual char DecimalPoint => '.';

    /// <inheritdoc />
    public virtual char IntegerDigitSeparator => '_';

    /// <inheritdoc />
    public virtual string ScientificNotationExponents => "eE";

    /// <inheritdoc />
    public virtual bool AllowNonIntegerNumbers => true;

    /// <inheritdoc />
    public virtual bool AllowScientificNotation => true;

    /// <inheritdoc />
    public virtual char StringDelimiter => '\'';

    /// <inheritdoc />
    public virtual bool ConvertResultToOutputTypeAutomatically => true;

    /// <inheritdoc />
    public virtual bool AllowNonPublicMemberAccess => false;

    /// <inheritdoc />
    public virtual bool IgnoreMemberNameCase => false;

    /// <inheritdoc />
    public virtual bool PostponeStaticInlineDelegateCompilation => false;

    /// <inheritdoc />
    public virtual bool DiscardUnusedArguments => true;
}
