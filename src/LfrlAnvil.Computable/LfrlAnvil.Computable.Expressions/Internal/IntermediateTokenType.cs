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

namespace LfrlAnvil.Computable.Expressions.Internal;

internal enum IntermediateTokenType : byte
{
    Argument = 0,
    NumberConstant = 1,
    StringConstant = 2,
    BooleanConstant = 3,
    Constructs = 4,
    OpenedParenthesis = 5,
    ClosedParenthesis = 6,
    OpenedSquareBracket = 7,
    ClosedSquareBracket = 8,
    LineSeparator = 9,
    ElementSeparator = 10,
    MemberAccess = 11,
    Assignment = 12,
    VariableDeclaration = 13,
    MacroDeclaration = 14
}
