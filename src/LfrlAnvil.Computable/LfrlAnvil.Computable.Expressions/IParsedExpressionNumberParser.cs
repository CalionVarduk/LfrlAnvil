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

using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Computable.Expressions;

/// <summary>
/// Represents a type-erased expression's number parser.
/// </summary>
public interface IParsedExpressionNumberParser
{
    /// <summary>
    /// Attempts to parse a number from the provided <paramref name="text"/>.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="result"><b>out</b> parameter that returns the result of parsing.</param>
    /// <returns><b>true</b> when parsing was successful, otherwise <b>false</b>.</returns>
    bool TryParse(StringSegment text, [MaybeNullWhen( false )] out object result);
}
