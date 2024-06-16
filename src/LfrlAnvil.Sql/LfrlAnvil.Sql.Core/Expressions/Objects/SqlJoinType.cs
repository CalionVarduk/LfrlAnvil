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

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents a type of record set join operation.
/// </summary>
public enum SqlJoinType : byte
{
    /// <summary>
    /// Represents an inner join.
    /// </summary>
    Inner = 0,

    /// <summary>
    /// Represents a left outer join.
    /// </summary>
    Left = 1,

    /// <summary>
    /// Represents a right outer join.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Represents a full outer join.
    /// </summary>
    Full = 3,

    /// <summary>
    /// Represents a cross join.
    /// </summary>
    Cross = 4
}
