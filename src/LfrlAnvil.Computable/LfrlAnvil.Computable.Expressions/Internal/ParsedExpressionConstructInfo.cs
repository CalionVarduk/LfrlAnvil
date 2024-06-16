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

using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Computable.Expressions.Internal;

/// <summary>
/// Represents information about a construct.
/// </summary>
public readonly struct ParsedExpressionConstructInfo
{
    internal ParsedExpressionConstructInfo(StringSegment symbol, ParsedExpressionConstructType type, object construct)
    {
        Symbol = symbol;
        Type = type;
        Construct = construct;
    }

    /// <summary>
    /// Construct's symbol.
    /// </summary>
    public StringSegment Symbol { get; }

    /// <summary>
    /// Construct's type.
    /// </summary>
    public ParsedExpressionConstructType Type { get; }

    /// <summary>
    /// Construct's instance.
    /// </summary>
    public object Construct { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="ParsedExpressionConstructInfo"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] '{Symbol}' -> {Construct.GetType().GetDebugString()}";
    }
}
