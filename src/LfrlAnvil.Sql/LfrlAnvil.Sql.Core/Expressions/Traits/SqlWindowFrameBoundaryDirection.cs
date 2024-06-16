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
/// Represents a direction of an <see cref="SqlWindowFrameBoundary"/>.
/// </summary>
public enum SqlWindowFrameBoundaryDirection : byte
{
    /// <summary>
    /// Specifies the current row.
    /// </summary>
    CurrentRow = 0,

    /// <summary>
    /// Specifies rows preceding the current row.
    /// </summary>
    Preceding = 1,

    /// <summary>
    /// Specifies rows following the current row.
    /// </summary>
    Following = 2
}
