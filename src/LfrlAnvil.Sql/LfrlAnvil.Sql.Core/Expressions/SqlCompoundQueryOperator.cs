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

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents a compound query operator.
/// </summary>
public enum SqlCompoundQueryOperator : byte
{
    /// <summary>
    /// Specifies a union operator. Only distinct records will be included.
    /// </summary>
    Union = 0,

    /// <summary>
    /// Specifies a union all operator. Non-distinct records will be included.
    /// </summary>
    UnionAll = 1,

    /// <summary>
    /// Specifies an intersect operator.
    /// </summary>
    Intersect = 2,

    /// <summary>
    /// Specifies an except operator.
    /// </summary>
    Except = 3
}
