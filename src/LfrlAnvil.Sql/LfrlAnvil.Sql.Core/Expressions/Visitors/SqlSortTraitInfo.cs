// Copyright 2026 Łukasz Furlepa
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

using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents extracted information about sort traits.
/// </summary>
/// <param name="Nodes">
/// Collection of ordering expressions that is the result of parsing of all <see cref="SqlSortTraitNode"/> instances.
/// </param>
/// <param name="Placeholder">Last encountered placeholder node.</param>
public readonly record struct SqlSortTraitInfo(Chain<ReadOnlyArray<SqlOrderByNode>> Nodes, SqlSortTraitPlaceholderNode? Placeholder);
