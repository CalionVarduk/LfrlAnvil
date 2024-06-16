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

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents the type of an <see cref="SqlWindowFrameNode"/>.
/// </summary>
public enum SqlWindowFrameType : byte
{
    /// <summary>
    /// Specifies a custom window frame type.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// Specifies that the frame's boundaries are determined by positions of rows relative to the current row.
    /// </summary>
    Rows = 1,

    /// <summary>
    /// Specifies that the frame's boundaries are determined by values of rows within range of the value of the current row.
    /// </summary>
    Range = 2
}
